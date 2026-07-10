using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Maps;
using PeePooFinder.Models;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private const double MapRadiusKm = 25;

    private readonly IPeePooApi _api;
    private readonly IGeolocation _geolocation;

    [ObservableProperty]
    private ObservableCollection<Place> places = new();

    public MapSpan? Region { get; private set; }

    public MapViewModel(IPeePooApi api, IGeolocation geolocation)
    {
        _api = api;
        _geolocation = geolocation;
        Title = "Map";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var query = new PlaceQuery();
            Location? location = null;
            try
            {
                location = await _geolocation.GetLastKnownLocationAsync()
                           ?? await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            }
            catch
            {
                // fall back to a non-geo listing when location is unavailable
            }

            if (location is not null)
            {
                query.Lat = location.Latitude;
                query.Long = location.Longitude;
                query.RadiusKm = MapRadiusKm;
                Region = MapSpan.FromCenterAndRadius(
                    new Location(location.Latitude, location.Longitude),
                    Distance.FromKilometers(6));
            }

            var results = await _api.GetPlacesAsync(query);
            Places = new ObservableCollection<Place>(results.Where(p => p.Lat != 0 || p.Long != 0));
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not load the map.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
