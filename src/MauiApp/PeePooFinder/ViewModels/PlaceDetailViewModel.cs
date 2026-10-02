using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

[QueryProperty(nameof(PlaceId), "id")]
public partial class PlaceDetailViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;
    private readonly AppSettings _settings;

    [ObservableProperty] private string placeId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveLabel), nameof(IsOwner), nameof(AvailabilityLabel), nameof(HasPlace))]
    private Place? place;

    [ObservableProperty] private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReviewButtonLabel))]
    private Review? myReview;

    [ObservableProperty] private bool hasReviews;

    public bool HasPlace => Place is not null;
    public bool IsOwner => Place?.IsOwner ?? false;
    public string SaveLabel => Place?.IsFavorite == true ? "Saved ✓" : "Save";
    public string ReviewButtonLabel => MyReview is null ? "Write a review" : "Edit your review";
    public string AvailabilityLabel => Place?.IsAvailable == true ? "Mark as closed" : "Mark as open";

    public PlaceDetailViewModel(IPeePooApi api, ISessionService session, AppSettings settings)
    {
        _api = api;
        _session = session;
        _settings = settings;
    }

    partial void OnPlaceIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(PlaceId)) return;
        await RunAsync(async () =>
        {
            var loaded = await _api.GetPlaceAsync(PlaceId);
            if (loaded is null)
            {
                await ShowError("This place isn't available anymore.");
                await Shell.Current.GoToAsync("..");
                return;
            }

            Place = loaded;
            Title = loaded.Name ?? "Place";
            var list = await _api.GetReviewsAsync(PlaceId);
            Reviews = new ObservableCollection<Review>(list);
            HasReviews = Reviews.Count > 0;
            MyReview = list.FirstOrDefault(r => r.IsMine);
        }, "Couldn't load this place.");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Place is null || !await EnsureSignedInAsync(_session, "save places")) return;
        await RunAsync(async () =>
        {
            Place.IsFavorite = await _api.ToggleFavoriteAsync(Place.Id);
            OnPropertyChanged(nameof(SaveLabel));
        }, "Couldn't update your saved places.", showBusy: false);
    }

    [RelayCommand]
    private async Task AddReviewAsync()
    {
        if (Place is null || !await EnsureSignedInAsync(_session, "write a review")) return;
        var route = MyReview is null
            ? $"{nameof(AddReviewPage)}?placeId={Place.Id}"
            : $"{nameof(AddReviewPage)}?placeId={Place.Id}&reviewId={MyReview.Id}";
        await Shell.Current.GoToAsync(route);
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (Place is null || !await EnsureSignedInAsync(_session, "confirm places")) return;
        await RunAsync(async () =>
        {
            await _api.VerifyPlaceAsync(Place.Id);
            Place = await _api.GetPlaceAsync(Place.Id) ?? Place;
            await ShowInfo("Thanks!", "You confirmed this place is still as described.");
        }, "Couldn't confirm this place.", showBusy: false);
    }

    [RelayCommand]
    private async Task DirectionsAsync()
    {
        if (Place is null) return;
        try
        {
            var options = new MapLaunchOptions { Name = Place.Name, NavigationMode = NavigationMode.Walking };
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(new Location(Place.Lat, Place.Long), options);
        }
        catch
        {
            await ShowError("Couldn't open your maps app.");
        }
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        if (Place is null) return;
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = Place.Name,
            Text = $"{Place.Name} — restroom on PeePoo Finder",
            Uri = _settings.Api.WebUrl($"lugar/{Place.Id}")
        });
    }

    [RelayCommand]
    private async Task ToggleAvailabilityAsync()
    {
        if (Place is null || !Place.IsOwner) return;
        var closing = Place.IsAvailable;
        if (closing && !await Confirm("Mark as closed", "People will see this place as closed until you reopen it.", "Mark closed"))
            return;

        await RunAsync(async () =>
        {
            await _api.SetAvailabilityAsync(Place.Id, !closing);
            Place = await _api.GetPlaceAsync(Place.Id) ?? Place;
        }, "Couldn't update this place.", showBusy: false);
    }

    [RelayCommand]
    private async Task DeletePlaceAsync()
    {
        if (Place is null || !Place.IsOwner) return;
        if (!await Confirm("Delete place", "This removes the place, its photos and all its reviews. It can't be undone.", "Delete"))
            return;

        await RunAsync(async () =>
        {
            await _api.DeletePlaceAsync(Place.Id);
            await Shell.Current.GoToAsync("..");
        }, "Couldn't delete this place.");
    }

    [RelayCommand]
    private async Task ReportPlaceAsync()
    {
        if (Place is null || !await EnsureSignedInAsync(_session, "report a place")) return;
        var reason = await AskReportReason();
        if (reason is null) return;
        await RunAsync(async () =>
        {
            await _api.ReportPlaceAsync(Place.Id, reason);
            await ShowInfo("Reported", "Thanks. Our team will take a look.");
        }, "Couldn't send the report.", showBusy: false);
    }

    [RelayCommand]
    private async Task ReviewActionsAsync(Review? review)
    {
        if (review is null || CurrentPage is null) return;

        if (review.IsMine)
        {
            var mine = await CurrentPage.DisplayActionSheetAsync("Your review", "Cancel", "Delete", "Edit");
            if (mine == "Edit") await AddReviewAsync();
            else if (mine == "Delete") await DeleteReviewAsync(review);
            return;
        }

        var action = await CurrentPage.DisplayActionSheetAsync("Review options", "Cancel", null, "Report review", $"Block @{review.Username}");
        if (action == "Report review") await ReportReviewAsync(review);
        else if (action?.StartsWith("Block") == true) await BlockAuthorAsync(review);
    }

    private async Task DeleteReviewAsync(Review review)
    {
        if (!await Confirm("Delete review", "Delete your review of this place?", "Delete")) return;
        await RunAsync(async () =>
        {
            await _api.DeleteReviewAsync(review.Id);
        }, "Couldn't delete your review.", showBusy: false);
        await LoadAsync();
    }

    private async Task ReportReviewAsync(Review review)
    {
        if (!await EnsureSignedInAsync(_session, "report a review")) return;
        var reason = await AskReportReason();
        if (reason is null) return;
        await RunAsync(async () =>
        {
            await _api.ReportReviewAsync(review.Id, reason);
            await ShowInfo("Reported", "Thanks. Our team will take a look.");
        }, "Couldn't send the report.", showBusy: false);
    }

    private async Task BlockAuthorAsync(Review review)
    {
        if (string.IsNullOrEmpty(review.Username) || !await EnsureSignedInAsync(_session, "block people")) return;
        if (!await Confirm("Block user", $"Hide everything @{review.Username} writes? You can undo this from your profile.", "Block"))
            return;
        await RunAsync(() => _api.BlockUserAsync(review.Username!), "Couldn't block this user.", showBusy: false);
        await LoadAsync();
    }
}
