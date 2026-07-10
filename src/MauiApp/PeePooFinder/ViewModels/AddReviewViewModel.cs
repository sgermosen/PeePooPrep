using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

[QueryProperty(nameof(PlaceId), "placeId")]
public partial class AddReviewViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly IMediaPicker _mediaPicker;

    private byte[]? _imageBytes;
    private string? _imageName;

    [ObservableProperty] private string placeId = string.Empty;
    [ObservableProperty] private string reviewTitle = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private double rating = 4;
    [ObservableProperty] private ImageSource? photoPreview;

    public AddReviewViewModel(IPeePooApi api, IMediaPicker mediaPicker)
    {
        _api = api;
        _mediaPicker = mediaPicker;
        Title = "Write a review";
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await _mediaPicker.PickPhotoAsync();
            if (photo is null) return;
            using var stream = await photo.OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            _imageBytes = memory.ToArray();
            _imageName = photo.FileName;
            PhotoPreview = ImageSource.FromStream(() => new MemoryStream(_imageBytes));
        }
        catch
        {
            await ShowError("Could not open the gallery.");
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(ReviewTitle) || string.IsNullOrWhiteSpace(Description))
        {
            await ShowError("Please add a title and a description.");
            return;
        }

        try
        {
            IsBusy = true;
            await _api.CreateReviewAsync(new SubmitReviewData
            {
                PlaceId = PlaceId,
                Title = ReviewTitle.Trim(),
                Description = Description.Trim(),
                Rating = (int)Math.Round(Rating),
                ImageBytes = _imageBytes,
                ImageName = _imageName
            });
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not post your review. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
