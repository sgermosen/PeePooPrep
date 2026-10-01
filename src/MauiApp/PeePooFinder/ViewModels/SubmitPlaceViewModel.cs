using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class SubmitPlaceViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly IGeolocation _geolocation;
    private readonly PhotoPicker _photo;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string selectedType = "Unisex";
    [ObservableProperty] private string observations = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string openingHours = string.Empty;
    [ObservableProperty] private int urinals;
    [ObservableProperty] private int toilets = 1;
    [ObservableProperty] private double rating = 4;
    [ObservableProperty] private bool haveBabyChanger;
    [ObservableProperty] private bool isRoomy;
    [ObservableProperty] private bool isAccessible;
    [ObservableProperty] private bool isFree = true;
    [ObservableProperty] private ImageSource? photoPreview;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLocation))]
    private Location? location;

    [ObservableProperty] private string locationLabel = "Finding your location…";

    public bool HasLocation => Location is not null;
    public IReadOnlyList<TypeOption> PlaceTypes { get; } =
        Models.PlaceTypes.All.Select(t => new TypeOption(t) { IsSelected = t == "Unisex" }).ToList();

    public SubmitPlaceViewModel(IPeePooApi api, IGeolocation geolocation, IMediaPicker mediaPicker)
    {
        _api = api;
        _geolocation = geolocation;
        _photo = new PhotoPicker(mediaPicker);
        Title = "Add a place";
    }

    [RelayCommand]
    private void SelectType(TypeOption? option)
    {
        if (option is null) return;
        foreach (var o in PlaceTypes) o.IsSelected = o == option;
        SelectedType = option.Name;
        if (option.Name == "Accessible") IsAccessible = true;
    }

    [RelayCommand]
    private async Task LocateAsync()
    {
        LocationLabel = "Finding your location…";
        try
        {
            // A fresh fix: you're adding the place you're standing in.
            var fix = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(15)))
                      ?? await _geolocation.GetLastKnownLocationAsync();
            Location = fix;
            LocationLabel = fix is null
                ? "We couldn't get your location. Turn it on and tap Retry."
                : $"Pinned where you are (±{Math.Round(fix.Accuracy ?? 0)} m)";
        }
        catch (PermissionException)
        {
            Location = null;
            LocationLabel = "Location permission is off. Allow it in Settings, then tap Retry.";
        }
        catch
        {
            Location = null;
            LocationLabel = "We couldn't get your location. Tap Retry.";
        }
    }

    [RelayCommand]
    private Task PickPhotoAsync() => SetPhotoAsync(_photo.PickAsync);

    [RelayCommand]
    private Task TakePhotoAsync() => SetPhotoAsync(_photo.CaptureAsync);

    [RelayCommand]
    private void RemovePhoto()
    {
        _photo.Clear();
        PhotoPreview = null;
    }

    private async Task SetPhotoAsync(Func<Task<ImageSource?>> source)
    {
        try
        {
            var preview = await source();
            if (preview is not null) PhotoPreview = preview;
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException)
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
        if (string.IsNullOrWhiteSpace(Name))
        {
            await ShowError("Give the place a name, e.g. \"Central Mall, 2nd floor\".");
            return;
        }

        if (Location is null)
        {
            await ShowError("We need your location to put this place on the map. Tap Retry next to the location.");
            return;
        }

        await RunAsync(async () =>
        {
            await _api.CreatePlaceAsync(new SubmitPlaceData
            {
                Name = Name.Trim(),
                Description = Description?.Trim() ?? string.Empty,
                Type = SelectedType,
                Observations = Observations?.Trim() ?? string.Empty,
                Address = Address?.Trim() ?? string.Empty,
                OpeningHours = OpeningHours?.Trim() ?? string.Empty,
                Rating = (int)Math.Round(Rating),
                Urinals = Urinals,
                Toilets = Toilets,
                HaveBabyChanger = HaveBabyChanger,
                IsRoomy = IsRoomy,
                IsAccessible = IsAccessible,
                IsFree = IsFree,
                Lat = Location.Latitude,
                Long = Location.Longitude,
                ImageBytes = _photo.Bytes,
                ImageName = _photo.FileName
            });

            await ShowInfo("Added", "Thanks! Your place is on the map.");
            await Shell.Current.GoToAsync("..");
        }, "Couldn't save the place. Please try again.");
    }
}

public partial class TypeOption : ObservableObject
{
    public TypeOption(string name) => Name = name;

    public string Name { get; }

    [ObservableProperty] private bool isSelected;
}
