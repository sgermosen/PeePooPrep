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

    [ObservableProperty]
    private string placeId = string.Empty;

    [ObservableProperty]
    private Place? place;

    [ObservableProperty]
    private ObservableCollection<Review> reviews = new();

    [ObservableProperty]
    private bool hasReviews;

    public PlaceDetailViewModel(IPeePooApi api)
    {
        _api = api;
    }

    partial void OnPlaceIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(PlaceId)) return;
        try
        {
            IsBusy = true;
            Place = await _api.GetPlaceAsync(PlaceId);
            Title = Place?.Name ?? "Place";
            var reviewList = await _api.GetReviewsAsync(PlaceId);
            Reviews = new ObservableCollection<Review>(reviewList.OrderByDescending(r => r.CreatedAt));
            HasReviews = Reviews.Count > 0;
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not load this place.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Place is null) return;
        try
        {
            await _api.ToggleFavoriteAsync(Place.Id);
            await ShowInfo("Saved", "Your favorites were updated.");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task AddReviewAsync()
    {
        if (Place is null) return;
        await Shell.Current.GoToAsync($"{nameof(AddReviewPage)}?placeId={Place.Id}");
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (Place is null) return;
        try
        {
            await _api.VerifyPlaceAsync(Place.Id);
            await LoadAsync();
            await ShowInfo("Thanks!", "You confirmed this spot is still good.");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task DirectionsAsync()
    {
        if (Place is null) return;
        try
        {
            var location = new Location(Place.Lat, Place.Long);
            var options = new MapLaunchOptions { Name = Place.Name, NavigationMode = NavigationMode.Walking };
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch
        {
            await ShowError("Could not open the maps app.");
        }
    }
}
