using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class SubmitPlaceViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly IGeolocation _geolocation;
    private readonly IMediaPicker _mediaPicker;

    private byte[]? _imageBytes;
    private string? _imageName;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string selectedType = "Unisex";
    [ObservableProperty] private string observations = string.Empty;
    [ObservableProperty] private int urinals;
    [ObservableProperty] private int toilets = 1;
    [ObservableProperty] private double rating = 4;
    [ObservableProperty] private bool haveBabyChanger;
    [ObservableProperty] private bool isRoomy;
    [ObservableProperty] private string locationLabel = "Locating you…";
    [ObservableProperty] private ImageSource? photoPreview;

    private double _lat;
    private double _long;

    public ObservableCollection<string> PlaceTypes { get; } = new()
    {
        "Unisex", "Men", "Women", "Family", "Accessible"
    };

    public SubmitPlaceViewModel(IPeePooApi api, IGeolocation geolocation, IMediaPicker mediaPicker)
    {
        _api = api;
        _geolocation = geolocation;
        _mediaPicker = mediaPicker;
        Title = "Add a spot";
    }

    [RelayCommand]
    private async Task LocateAsync()
    {
        try
        {
            LocationLabel = "Locating you…";
            var location = await _geolocation.GetLastKnownLocationAsync()
                           ?? await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15)));
            if (location is not null)
            {
                _lat = location.Latitude;
                _long = location.Longitude;
                LocationLabel = $"{_lat:0.0000}, {_long:0.0000}";
            }
            else
            {
                LocationLabel = "Location unavailable";
            }
        }
        catch
        {
            LocationLabel = "Location unavailable";
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await _mediaPicker.PickPhotoAsync();
            await LoadPhoto(photo);
        }
        catch
        {
            await ShowError("Could not open the gallery.");
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!_mediaPicker.IsCaptureSupported)
            {
                await ShowError("Camera is not available on this device.");
                return;
            }
            var photo = await _mediaPicker.CapturePhotoAsync();
            await LoadPhoto(photo);
        }
        catch
        {
            await ShowError("Could not open the camera.");
        }
    }

    private async Task LoadPhoto(FileResult? photo)
    {
        if (photo is null) return;
        using var stream = await photo.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        _imageBytes = memory.ToArray();
        _imageName = photo.FileName;
        PhotoPreview = ImageSource.FromStream(() => new MemoryStream(_imageBytes));
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description))
        {
            await ShowError("Please give the spot a name and a short description.");
            return;
        }

        try
        {
            IsBusy = true;
            await _api.CreatePlaceAsync(new SubmitPlaceData
            {
                Name = Name.Trim(),
                Description = Description.Trim(),
                Type = SelectedType,
                Observations = Observations?.Trim() ?? string.Empty,
                Rating = (int)Math.Round(Rating),
                Urinals = Urinals,
                Toilets = Toilets,
                HaveBabyChanger = HaveBabyChanger,
                IsRoomy = IsRoomy,
                Lat = _lat,
                Long = _long,
                ImageBytes = _imageBytes,
                ImageName = _imageName
            });

            await ShowInfo("Thanks!", "Your spot was added to the map.");
            await Shell.Current.GoToAsync("//main");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not save the spot. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
