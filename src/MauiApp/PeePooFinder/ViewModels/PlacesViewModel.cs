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
    private readonly List<Place> _allPlaces = new();

    [ObservableProperty]
    private ObservableCollection<Place> places = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool nearMe = true;

    [ObservableProperty]
    private bool onlyBabyChanger;

    [ObservableProperty]
    private bool onlyAvailable;

    public PlacesViewModel(IPeePooApi api, IGeolocation geolocation)
    {
        _api = api;
        _geolocation = geolocation;
        Title = "Nearby spots";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var query = new PlaceQuery
            {
                BabyChanger = OnlyBabyChanger ? true : null,
                AvailableOnly = OnlyAvailable ? true : null
            };

            if (NearMe)
            {
                var location = await GetLocationAsync();
                if (location is not null)
                {
                    query.Lat = location.Latitude;
                    query.Long = location.Longitude;
                    query.RadiusKm = SearchRadiusKm;
                }
            }

            var results = await _api.GetPlacesAsync(query);
            _allPlaces.Clear();
            _allPlaces.AddRange(results);
            ApplyFilter();
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not load places. Pull to refresh to try again.");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
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

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnNearMeChanged(bool value) => _ = LoadAsync();
    partial void OnOnlyBabyChangerChanged(bool value) => _ = LoadAsync();
    partial void OnOnlyAvailableChanged(bool value) => _ = LoadAsync();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPlaces
            : _allPlaces.Where(p =>
                (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Type?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        Places = new ObservableCollection<Place>(filtered);
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Place? place)
    {
        if (place is null) return;
        await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={place.Id}");
    }

    [RelayCommand]
    private static Task AddPlaceAsync() => Shell.Current.GoToAsync(nameof(SubmitPlacePage));
}
