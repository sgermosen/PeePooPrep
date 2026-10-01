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
    private static readonly Location DefaultCenter = new(18.4861, -69.9312);

    private readonly IPeePooApi _api;
    private readonly IGeolocation _geolocation;

    [ObservableProperty] private ObservableCollection<Place> places = new();

    public MapSpan Region { get; private set; } = MapSpan.FromCenterAndRadius(DefaultCenter, Distance.FromKilometers(8));

    public MapViewModel(IPeePooApi api, IGeolocation geolocation)
    {
        _api = api;
        _geolocation = geolocation;
        Title = "Map";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var query = new PlaceQuery { Limit = 300 };
            Location? location = null;
            try
            {
                location = await _geolocation.GetLastKnownLocationAsync()
                           ?? await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            }
            catch
            {
                // Without location we show everything around the default city.
            }

            if (location is not null)
            {
                query.Lat = location.Latitude;
                query.Long = location.Longitude;
                query.RadiusKm = MapRadiusKm;
                Region = MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude), Distance.FromKilometers(3));
            }

            Places = new ObservableCollection<Place>(await _api.GetPlacesAsync(query));
        }, "Couldn't load the map.");
    }
}
