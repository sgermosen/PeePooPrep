using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

/// <summary>Writes a new review, or edits yours when a reviewId is passed.</summary>
[QueryProperty(nameof(PlaceId), "placeId")]
[QueryProperty(nameof(ReviewId), "reviewId")]
public partial class AddReviewViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly PhotoPicker _photo;

    [ObservableProperty] private string placeId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(CanAddPhoto), nameof(SubmitLabel))]
    private string? reviewId;

    [ObservableProperty] private string reviewTitle = string.Empty;
    [ObservableProperty] private string description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RatingLabel))]
    private double rating = 4;

    [ObservableProperty] private ImageSource? photoPreview;

    public bool IsEditing => !string.IsNullOrEmpty(ReviewId);
    public bool CanAddPhoto => !IsEditing;
    public string SubmitLabel => IsEditing ? "Save changes" : "Post review";
    public string RatingLabel => (int)Math.Round(Rating) switch
    {
        5 => "5 · I'd come back on purpose",
        4 => "4 · Clean, no hassle",
        3 => "3 · Does the job",
        2 => "2 · Only in an emergency",
        _ => "1 · Avoid"
    };

    public AddReviewViewModel(IPeePooApi api, IMediaPicker mediaPicker)
    {
        _api = api;
        _photo = new PhotoPicker(mediaPicker);
        Title = "Your review";
    }

    partial void OnReviewIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadExistingAsync(value);
    }

    private Task LoadExistingAsync(string id) => RunAsync(async () =>
    {
        var review = await _api.GetReviewAsync(id);
        if (review is null) return;
        ReviewTitle = review.Title ?? string.Empty;
        Description = review.Description ?? string.Empty;
        Rating = review.Rating;
    }, "Couldn't load your review.");

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var preview = await _photo.PickAsync();
            if (preview is not null) PhotoPreview = preview;
        }
        catch (InvalidOperationException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Couldn't open your photos.");
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(ReviewTitle) || string.IsNullOrWhiteSpace(Description))
        {
            await ShowError("Add a short title and tell people how it was.");
            return;
        }

        var stars = (int)Math.Round(Rating);
        await RunAsync(async () =>
        {
            if (IsEditing)
            {
                await _api.UpdateReviewAsync(ReviewId!, ReviewTitle.Trim(), Description.Trim(), stars);
            }
            else
            {
                await _api.CreateReviewAsync(new SubmitReviewData
                {
                    PlaceId = PlaceId,
                    Title = ReviewTitle.Trim(),
                    Description = Description.Trim(),
                    Rating = stars,
                    ImageBytes = _photo.Bytes,
                    ImageName = _photo.FileName
                });
            }
            await Shell.Current.GoToAsync("..");
        }, "Couldn't save your review. Please try again.");
    }
}
