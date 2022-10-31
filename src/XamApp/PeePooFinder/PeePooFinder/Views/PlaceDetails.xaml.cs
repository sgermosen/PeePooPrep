using Newtonsoft.Json;
using PeePooFinder.Models;
using PeePooFinder.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using Toast = PeePooFinder.Services.Toast;
namespace PeePooFinder.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // [QueryProperty(nameof(PlaceID), "PlaceID")]
    public partial class PlaceDetails : ContentPage
    {
        PlaceDetailViewModel _viewModel;
        List<Places> places;
        private FileResult fileresult { get; set; }
        private Stream pickedFileStram { get; set; }
        public PlaceDetails()
        {
            InitializeComponent();
            this.BindingContext = _viewModel = new PlaceDetailViewModel();
            //Thread.Sleep(2000);
            //LoadMap();
        }

        private async void PhotoTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                fileresult = null;
                pickedFileStram = null;
                fileresult = await MediaPicker.CapturePhotoAsync();
                if (fileresult != null)
                {
                    pickedFileStram = await fileresult.OpenReadAsync();
                    if (fileresult != null && pickedFileStram != null)
                    {

                        BtnSelectPhoto.Text = "Uploading";
                        imgProfile.Source = fileresult.FileName;
                    }
                }
            }
            catch (Exception ex)
            {

                BtnSelectPhoto.Text = "Select";
                await App.Current.MainPage.DisplayAlert("Error", "Error in processing, please try again", "OK");
            }
        }

        private async void BtnTakePhoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                fileresult = null;
                pickedFileStram = null;
                fileresult = await FilePicker.PickAsync(new PickOptions()
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Please select file"
                });
                string[] fileTypes = null;
                if (Device.RuntimePlatform == Device.Android)
                    fileTypes = new string[] { "image/png", "image/jpeg", "image/.png" };

                if (fileresult != null)
                {
                    pickedFileStram = await fileresult.OpenReadAsync();
                    ImageSource.FromStream(() => pickedFileStram);
                    if (fileresult != null && pickedFileStram != null)
                    {
                        if (fileresult.FileName.ToLower().Trim().EndsWith(".jpg") || fileresult.FileName.ToLower().Trim().EndsWith(".jpeg")
                            || fileresult.FileName.ToLower().Trim().EndsWith(".png"))
                        {
                            BtnSelectPhoto.Text = "uploading";
                            imgProfile.Source = fileresult.FullPath;
                        }
                        else
                        {
                            await DisplayAlert("Image file required", "Please Select image (JPG or PNG)", "OK");
                            BtnSelectPhoto.Text = "Select";
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BtnSelectPhoto.Text = "Select";
                await App.Current.MainPage.DisplayAlert("Error", "Error in processing, please try again", "OK");
            }
        }

        private async void LoadMap()
        {
            _viewModel.GetPlaceLatLong(_viewModel.PlaceID);
            Pin curPin = new Pin
            {
                Type = PinType.Place,
                Label = _viewModel.Name,
                Address = _viewModel.Description,
                Position = new Position(_viewModel.LstLatLong[0], _viewModel.LstLatLong[1]),
                Tag = "id_saved_" + _viewModel.PlaceID
            };
            map.Pins.Add(curPin);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(curPin.Position, Distance.FromMeters(5000)));
        }
        protected override async void OnAppearing()
        {
            try
            {
                List<double> lstLatLong = await _viewModel.MyPlaceService.GetPlaceLocation(_viewModel.PlaceID);
                Pin curPin = new Pin
                {
                    Type = PinType.Place,
                    Label = string.IsNullOrEmpty(_viewModel.Name)?"": _viewModel.Name,
                    Address = string.IsNullOrEmpty(_viewModel.Description) ? "" : _viewModel.Description,
                    Position = new Position(lstLatLong[0], lstLatLong[1]),
                    Tag = "id_saved_" + _viewModel.PlaceID
                };
                map.Pins.Add(curPin);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(curPin.Position, Distance.FromMeters(5000)));
                Thread.Sleep(2000);
                BindPlaceReviews();
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }
        public async void BindPlaceReviews()
        {
            try
            {
                List<Visit> lstVisits = await _viewModel.MyPlaceService.GetVisitsByPlaceID(_viewModel.PlaceID);
                if (lstVisits != null && lstVisits.Count > 0)
                {
                    foreach (Visit visit in lstVisits)
                    {
                        Label lblUserName = new Label
                        {
                            Text = string.IsNullOrEmpty(visit.displayName) ? "Anonymous User" : visit.displayName,
                            Margin = new Thickness(10, 0, 0, 0),
                            TextColor = Color.FromHex("#BF6952"),
                            FontSize = 16,
                            BackgroundColor = Color.Transparent
                        };
                        stkReviews.Children.Add(lblUserName);
                        StackLayout stkRatings = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        int Maxrate = 5;
                        int RemRating = 0;
                        for (int x = 0; x < visit.rating; x++)
                        {
                            Image img = new Image
                            {
                                HorizontalOptions = LayoutOptions.Start,
                                Source = "StarYellow.png",
                                HeightRequest = 30,
                                WidthRequest = 30,
                                Margin = new Thickness(0, 0, 5, 0)
                            };
                            stkRatings.Children.Add(img);
                        }
                        if ((Maxrate - visit.rating) > 0)
                        {
                            for (int y = 0; y < (Maxrate - visit.rating); y++)
                            {
                                Image img = new Image
                                {
                                    HorizontalOptions = LayoutOptions.Start,
                                    Source = "emptystar.png",
                                    HeightRequest = 30,
                                    WidthRequest = 30,
                                    Margin = new Thickness(0, 0, 5, 0)
                                };
                                stkRatings.Children.Add(img);
                            }
                        }
                        stkReviews.Children.Add(stkRatings);
                        StackLayout stkReview = new StackLayout
                        {
                        };

                        Label lblReview = new Label
                        {
                            Text = visit.description,
                            Padding = new Thickness(5)
                        };
                        stkReview.Children.Add(lblReview);
                        stkReviews.Children.Add(stkReview);
                        if (visit.photos != null && visit.photos.Count > 0)
                        {
                            StackLayout stkImagesLabel = new StackLayout
                            {
                            };
                            Label lblImageLabel = new Label
                            {
                                Text = "Images"
                            };
                            stkReview.Children.Add(lblImageLabel);
                            stkReviews.Children.Add(stkImagesLabel);
                            ScrollView srcImages = new ScrollView
                            {
                                Orientation = ScrollOrientation.Horizontal
                            };
                            StackLayout stkImageContainer = new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal
                            };

                            for (int m = 0; m < visit.photos.Count; m++)
                            {
                                Image imgReview = new Image
                                {
                                    Source = visit.photos[m].url,
                                    HeightRequest = 60,
                                    Margin = new Thickness(0, 0, 5, 0)
                                };
                                stkImageContainer.Children.Add(imgReview);
                            }
                            srcImages.Content = stkImageContainer;
                            stkReviews.Children.Add(srcImages);
                        }
                        BoxView boxView = new BoxView
                        {
                            Margin = new Thickness(0, 10, 0, 0),
                            HeightRequest = 1,
                            BackgroundColor = Color.FromHex("#dadada"),
                            HorizontalOptions = LayoutOptions.FillAndExpand
                        };
                        stkReviews.Children.Add(boxView);
                    }
                }
                else
                {

                    Label lblNoReviews = new Label
                    {
                        Text = "No Reviews Yet",
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        TextColor = Color.FromHex("#BF6952"),
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold
                    };
                    stkReviews.Children.Add(lblNoReviews);
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void map_PinClicked(object sender, PinClickedEventArgs e)
        {
            try
            {
                //var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20));
                //CancellationTokenSource cts = new CancellationTokenSource();
                //var location = await Geolocation.GetLocationAsync(request, cts.Token);

                var location = await Geolocation.GetLastKnownLocationAsync();
                string sourceAddress = "";
                string destinationAddress = "";
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        sourceAddress = string.IsNullOrEmpty(placemark.SubLocality) ? placemark.SubLocality : placemark.Locality + "," + placemark.CountryName;
                    }
                }
                var placemarkd = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(lblLat.Text), Convert.ToDouble(lblLong.Text));
                var placemarkdad = placemarkd?.FirstOrDefault();
                if (placemarkdad != null)
                {
                    destinationAddress = string.IsNullOrEmpty(placemarkdad.SubLocality) ? placemarkdad.SubLocality : placemarkdad.Locality + "," + placemarkdad.CountryName;
                }

                var supportsUri = await Launcher.CanOpenAsync("comgooglemaps://");

                //if (supportsUri)
                //    await Launcher.OpenAsync($"comgooglemaps://?q={lblLat.Text},{lblLong.Text}({_viewModel.Name})");

                //else
                //    await Map.OpenAsync(40.765819, -73.975866, new MapLaunchOptions { Name = "Test" });

                if (Device.RuntimePlatform == Device.iOS)
                {
                    await Launcher.OpenAsync("http://maps.apple.com/?daddr=" + destinationAddress.Replace(" ", "+") + ",&saddr=" + sourceAddress.Replace(" ", "+") + "");
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    // opens the 'task chooser' so the user can pick Maps, Chrome or other mapping app
                    await Launcher.OpenAsync("https://www.google.com/maps/@" + lblLat.Text + "," + lblLong.Text + ",5z");
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void btnShowDirections_Clicked(object sender, EventArgs e)
        {
            try
            {
                //var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20));
                //CancellationTokenSource cts = new CancellationTokenSource();
                //var location = await Geolocation.GetLocationAsync(request, cts.Token);

                var location = await Geolocation.GetLastKnownLocationAsync();
                string sourceAddress = "";
                string destinationAddress = "";
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        sourceAddress = string.IsNullOrEmpty(placemark.SubLocality) ? placemark.SubLocality : placemark.Locality + "," + placemark.CountryName;
                    }
                }
                var placemarkd= await Geocoding.GetPlacemarksAsync(Convert.ToDouble(lblLat.Text), Convert.ToDouble(lblLong.Text));
                var placemarkdad = placemarkd?.FirstOrDefault();
                if (placemarkdad != null)
                {
                    destinationAddress = string.IsNullOrEmpty(placemarkdad.SubLocality) ? placemarkdad.SubLocality : placemarkdad.Locality + "," + placemarkdad.CountryName;
                }

                var supportsUri = await Launcher.CanOpenAsync("comgooglemaps://");

                //if (supportsUri)
                //    await Launcher.OpenAsync($"comgooglemaps://?q={lblLat.Text},{lblLong.Text}({_viewModel.Name})");

                //else
                //    await Map.OpenAsync(40.765819, -73.975866, new MapLaunchOptions { Name = "Test" });

                if (Device.RuntimePlatform == Device.iOS)
                {
                    await Launcher.OpenAsync("http://maps.apple.com/?daddr=" + destinationAddress.Replace(" ", "+") + ",&saddr=" + sourceAddress.Replace(" ", "+") + "");
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    // opens the 'task chooser' so the user can pick Maps, Chrome or other mapping app
                    await Launcher.OpenAsync("https://www.google.com/maps/@" + lblLat.Text + "," + lblLong.Text + ",5z");
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
           
        }

        private async void LblSubmitedBy_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(UserPage));
        }

        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            //Application.Current.MainPage = new AppShell();
            //private async void CancelButton_Clicked(object sender, EventArgs e)
            //{
                var shell = Application.Current.MainPage as AppShell;
                if (shell != null)
                {
                    shell.Navigation.PopAsync();
                }
            //}
        }

        private async void btnReview_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(CommentsPage)}?CurPlaceID={_viewModel.PlaceID.Replace("id_saved_", "")}");
        }
    }
}