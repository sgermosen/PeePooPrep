using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Essentials;
using PeePooFinder.Models;
using PeePooFinder.ViewModels;
using Newtonsoft.Json;
using Acr.UserDialogs;
using Toast = PeePooFinder.Services.Toast;
using System.Threading;

namespace PeePooFinder.Views
{
    [DesignTimeVisible(false)]
    public partial class HomePage : ContentPage
    {
        HomePageViewModel _viewModel;
        Places placeDetail;
        string placeID;
        List<Places> places;
        bool isLoading;
        public HomePage()
        {
            InitializeComponent();
            places = new List<Places>();
            _viewModel = new HomePageViewModel();
            //var location = Geolocation.GetLastKnownLocationAsync().Result;
            //if (location != null)
            //{
            //}

          //  LoadMap();
        }
        public HomePage(LoginResponseModel objResponse)
        {
            InitializeComponent();
            _viewModel = new HomePageViewModel();
            //var location = Geolocation.GetLocationAsync().Result;
            //if (location != null)
            //{
            //}
           // LoadMap();
        }
        protected override void OnAppearing()
        {
            using (UserDialogs.Instance.Loading("Loading Places data..."))
            {
                base.OnAppearing();
                if (_viewModel.Places != null || _viewModel.Places.Count <= 0)
                {
                    _viewModel = new HomePageViewModel();
                }
                //_viewModel.OnAppearing();
                Thread.Sleep(1500);
                if (!isLoading)
                {
                    LoadMap();
                }
            }
        }

        private async void LoadMap()
        {
            try
            {
                if (!isLoading)
                {
                    isLoading = true;
                    using (UserDialogs.Instance.Loading("Loading Place data..."))
                    {
                        places = await _viewModel.MyPlaceService.GetAllPlacesAsync(true);

                        List<Pin> pins = new List<Pin>();
                        if (places != null && places.Count > 0)
                        {   
                            List<Places> curplaces = places.Where(x => x.lat > 0 || x.@long > 0).ToList();
                            if (curplaces != null && curplaces.Count > 0)
                            {
                                if (map.Pins.Count > 0)
                                {
                                    map = new Xamarin.Forms.GoogleMaps.Map();
                                }
                                foreach (Places place in curplaces)
                                {
                                    Pin curPin = new Pin
                                    {
                                        Type = PinType.Place,
                                        Label = place.name,
                                        Address = place.description,
                                        Position = new Position(place.lat, place.@long),
                                        Tag = "id_saved_" + place.id,
                                    };
                                    pins.Add(curPin);
                                    map.Pins.Add(curPin);
                                }
                                var location = await Geolocation.GetLastKnownLocationAsync();
                                if (location != null)
                                {
                                    if (!Application.Current.Properties.ContainsKey("FirstLoad"))
                                    {
                                        Application.Current.Properties.Add("FirstLoad", "Yes");
                                        Application.Current.MainPage = new AppShell();

                                    }
                                    Pin curLocPin = new Pin
                                    {
                                        Type = PinType.Place,
                                        Label = "",
                                        Address = "",
                                        Position = new Position(location.Latitude, location.Longitude),
                                        Tag = "id_saved_Cur",
                                    };
                                    map.MoveToRegion(MapSpan.FromCenterAndRadius(curLocPin.Position, Distance.FromMeters(5000)));
                                }
                                else
                                {
                                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pins[0].Position, Distance.FromMeters(5000)));
                                }
                                
                            }
                        }
                    }
                    isLoading = false;
                }
            }
            catch (Exception ex)
            {

                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void map_MapClicked(object sender, MapClickedEventArgs e)
        {
            Position clickPosition = e.Point;
            if (clickPosition != null)
            {
                Pin newp = map.Pins.Where(x => x.Tag.ToString().Contains("_new_")).FirstOrDefault();
                if (newp != null)
                {
                    map.Pins.Remove(newp);
                }
                stkNewPlace.IsVisible = false;
                var placemarks = await Geocoding.GetPlacemarksAsync(clickPosition.Latitude, clickPosition.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    Pin pinTokyo = new Pin()
                    {
                        Type = PinType.Place,
                        //Label = string.IsNullOrEmpty(placemark.SubLocality)?placemark.CountryName: placemark.SubLocality,
                        Label = "Submit a place here",
                        //Address = placemark.Locality + "," + placemark.CountryName,
                        Address = "",
                        Position = clickPosition,
                        Rotation = 0.0f,
                        Tag = "id_new_" + Guid.NewGuid().ToString().Replace("-", "_"),
                    };
                    map.Pins.Add(pinTokyo);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pinTokyo.Position, Distance.FromMeters(5000)));
                }
                else
                { 
                    Pin pinTokyo = new Pin()
                    {
                        Type = PinType.Place,
                        //Label = string.IsNullOrEmpty(placemark.SubLocality)?placemark.CountryName: placemark.SubLocality,
                        Label = "Submit a place here",
                        //Address = placemark.Locality + "," + placemark.CountryName,
                        Address = "",
                        Position = clickPosition,
                        Rotation = 0.0f,
                        Tag = "id_new_" + Guid.NewGuid().ToString().Replace("-", "_"),
                    };
                    map.Pins.Add(pinTokyo);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pinTokyo.Position, Distance.FromMeters(5000)));
                }
            }
        }

        private async void map_PinClicked(object sender, PinClickedEventArgs e)
        {
            try
            {
                // await Launcher.OpenAsync("geo:0,0?q=394+Pacific+Ave+San+Francisco+CA");
                using (UserDialogs.Instance.Loading("Loading Place data..."))
                {
                    if (e.Pin.Tag != null && e.Pin.Tag.ToString().Contains("_new_"))
                    {
                        placeDetail = new Places
                        {
                            lat = e.Pin.Position.Latitude,
                            @long = e.Pin.Position.Longitude
                        };
                        stkViewPlace.IsVisible = false;
                        stkNewPlace.IsVisible = true;
                    }
                    else
                    {
                        stkNewPlace.IsVisible = false;
                        stkViewPlace.IsVisible = true;
                        placeID = e.Pin.Tag.ToString().Replace("id_saved_", "");
                    }
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void btnSubmitPlace_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Please wait..."))
            {
                var placestr = JsonConvert.SerializeObject(placeDetail);
                await Shell.Current.GoToAsync($"{nameof(SubmitPlace)}?PlaceDetail={placestr}");
            }
            //try
            //{
            //    Pin newp = map.Pins.Where(x => x.Tag.ToString().Contains("_new_")).FirstOrDefault();

            //    foreach (Pin p in map.Pins)
            //    {
            //        if (p != null && p.Tag != null && p.Tag.ToString().Contains("_new_"))
            //        {
            //            map.Pins.Remove(p);
            //        }
            //    }
            //    DependencyService.Get<Toast>().Show("All Unsaved pins have been removed");
            //}
            //catch (Exception ex)
            //{
            //    DependencyService.Get<Toast>().Show("Some error occured in removing unsaved pins");
            //}
        }

        private async void btnViewPlace_Clicked(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading Data..."))
            {
                var placestr = JsonConvert.SerializeObject(placeDetail);
                await Shell.Current.GoToAsync($"{nameof(PlaceDetails)}?PlaceID={placeID.Replace("id_saved_", "")}");
            }
        }

        private async void map_InfoWindowClicked(object sender, InfoWindowClickedEventArgs e)
        {
            using (UserDialogs.Instance.Loading("Loading Place data..."))
            {
                if (e.Pin.Tag != null && e.Pin.Tag.ToString().Contains("_new_"))
                {
                    placeDetail = new Places
                    {
                        lat = e.Pin.Position.Latitude,
                        @long = e.Pin.Position.Longitude
                    };
                    stkViewPlace.IsVisible = false;
                    stkNewPlace.IsVisible = true;
                    using (UserDialogs.Instance.Loading("Please wait..."))
                    {
                        var placestr = JsonConvert.SerializeObject(placeDetail);
                        await Shell.Current.GoToAsync($"{nameof(SubmitPlace)}?PlaceDetail={placestr}");
                    }
                }
                else
                {
                    stkNewPlace.IsVisible = false;
                    stkViewPlace.IsVisible = true;
                    placeID = e.Pin.Tag.ToString().Replace("id_saved_", "");
                    using (UserDialogs.Instance.Loading("Loading Data..."))
                    {
                        var placestr = JsonConvert.SerializeObject(placeDetail);
                        await Shell.Current.GoToAsync($"{nameof(PlaceDetails)}?PlaceID={placeID.Replace("id_saved_", "")}");
                    }
                }
            }
        }
    }
}