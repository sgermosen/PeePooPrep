using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

public partial class PlacesViewModel : BaseViewModel
{
    private const double SearchRadiusKm = 15;

    private readonly IPeePooApi _api;
    private readonly IGeolocation _geolocation;
    private readonly ISessionService _session;
    private bool _loadedOnce;

    [ObservableProperty] private ObservableCollection<Place> places = new();
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool onlyAccessible;
    [ObservableProperty] private bool onlyBabyChanger;
    [ObservableProperty] private bool onlyFree;
    [ObservableProperty] private bool onlyOpen;
    [ObservableProperty] private string statusText = string.Empty;
    [ObservableProperty] private bool usingLocation;

    public PlacesViewModel(IPeePooApi api, IGeolocation geolocation, ISessionService session)
    {
        _api = api;
        _geolocation = geolocation;
        _session = session;
        Title = "Nearby";
    }

    public async Task LoadIfNeededAsync()
    {
        if (_loadedOnce) return;
        _loadedOnce = true;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var query = new PlaceQuery
            {
                Search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                Accessible = OnlyAccessible ? true : null,
                BabyChanger = OnlyBabyChanger ? true : null,
                Free = OnlyFree ? true : null,
                AvailableOnly = OnlyOpen ? true : null,
                Limit = 200
            };

            var location = await GetLocationAsync();
            UsingLocation = location is not null;
            if (location is not null)
            {
                query.Lat = location.Latitude;
                query.Long = location.Longitude;
                // A text search looks further afield than the default nearby radius.
                query.RadiusKm = string.IsNullOrWhiteSpace(SearchText) ? SearchRadiusKm : 100;
            }
            else
            {
                query.Sort = "rating";
            }

            var results = await _api.GetPlacesAsync(query);
            Places = new ObservableCollection<Place>(results);
            StatusText = results.Count == 0
                ? string.Empty
                : UsingLocation ? $"{results.Count} nearby · closest first" : $"{results.Count} places · best rated first (turn on location to sort by distance)";
        }, "Couldn't load places. Pull down to try again.");
        IsRefreshing = false;
    }

    private async Task<Location?> GetLocationAsync()
    {
        try
        {
            return await _geolocation.GetLastKnownLocationAsync()
                   ?? await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
        }
        catch
        {
            return null;
        }
    }

    partial void OnOnlyAccessibleChanged(bool value) => _ = LoadAsync();
    partial void OnOnlyBabyChangerChanged(bool value) => _ = LoadAsync();
    partial void OnOnlyFreeChanged(bool value) => _ = LoadAsync();
    partial void OnOnlyOpenChanged(bool value) => _ = LoadAsync();

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    [RelayCommand] private void ToggleAccessible() => OnlyAccessible = !OnlyAccessible;
    [RelayCommand] private void ToggleBabyChanger() => OnlyBabyChanger = !OnlyBabyChanger;
    [RelayCommand] private void ToggleFree() => OnlyFree = !OnlyFree;
    [RelayCommand] private void ToggleOpen() => OnlyOpen = !OnlyOpen;

    [RelayCommand]
    private static async Task GoToDetailAsync(Place? place)
    {
        if (place is null) return;
        await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={place.Id}");
    }

    [RelayCommand]
    private async Task AddPlaceAsync()
    {
        if (!await EnsureSignedInAsync(_session, "add a place")) return;
        await Shell.Current.GoToAsync(nameof(SubmitPlacePage));
    }
}
