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
using Toast = PeePooFinder.Services.Toast;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using PeePooFinder.Models;

namespace PeePooFinder.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentsPage : ContentPage
    {
        private FileResult fileresult { get; set; }
        private Stream pickedFileStream { get; set; }
        private byte[] imageBytes { get; set; }
        CommentsViewModel _viewModel;
        public CommentsPage()
        {
            InitializeComponent();
            //imgProfile.Source = "LogoTransparent.png";
            //this.BindingContext = new PlacesViewModel();
            this.BindingContext = _viewModel = new CommentsViewModel();
            _viewModel.OwnerUserName = Convert.ToString(Application.Current.Properties["UserName"]);
        }

        private async void PhotoTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                fileresult = null;
                pickedFileStream = null;
                fileresult = await MediaPicker.CapturePhotoAsync();
                if (fileresult != null)
                {
                    _viewModel.ImgStream = await fileresult.OpenReadAsync();
                    if (fileresult != null && _viewModel.ImgStream != null)
                    {
                        _viewModel.ImageName = fileresult.FileName.ToString();

                        using (MemoryStream memory = new MemoryStream())
                        {
                            Stream stream = _viewModel.ImgStream;
                            stream.CopyTo(memory);
                            imageBytes = memory.ToArray();
                            _viewModel.Image = imageBytes;
                            _viewModel.ImageName = fileresult.FileName.ToString();
                        }
                        Thread.Sleep(2000);
                        ImgPlace.Source = ImageSource.FromStream(() => fileresult.OpenReadAsync().Result);
                        ImgPlace.IsVisible = true;
                    }
                    else
                    {
                        ImgPlace.IsVisible = false;
                    }
                }
                else
                {
                    ImgPlace.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error in processing, please try again", "OK");
            }
        }

        private async void btnSubmitReview_Clicked(object sender, EventArgs e)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(_viewModel.CommentTitle)
                    || string.IsNullOrEmpty(_viewModel.CommentDescription)
                    )
                {
                    msg = "Please fill all the details and try again";
                    DependencyService.Get<Toast>().Show(msg);
                }
                //else if (string.IsNullOrEmpty(_viewModel.ImageName))
                //{
                //    DependencyService.Get<Toast>().Show("Please take an image before submit");
                //}
                else
                {
                    using (UserDialogs.Instance.Loading("Saving Comment data..."))
                    {
                        Visits objVisit = new Visits
                        {
                            id = Guid.NewGuid().ToString(),
                            title = _viewModel.CommentTitle,
                            placeId = _viewModel.CurPlaceID,
                            description = _viewModel.CommentDescription,
                            createdAt = System.DateTime.Now,
                            rating = _viewModel.Rating,
                            ImageName = _viewModel.ImageName,
                            ImageBytes = _viewModel.Image,
                            OwnerUserName = _viewModel.OwnerUserName
                        };
                        var objGetPlaceDetails = await _viewModel.MyPlaceService.SubmitComment(objVisit);
                        if (objGetPlaceDetails.Status == "success")
                        {
                            DependencyService.Get<Toast>().Show(objGetPlaceDetails.Message);
                            Thread.Sleep(1500);
                            Application.Current.MainPage = new AppShell();
                            //await Shell.Current.Navigation.PopToRootAsync();
                        }
                        else
                        {
                            DependencyService.Get<Toast>().Show(objGetPlaceDetails.Message);
                        }
                        //await Shell.Current.GoToAsync(nameof(HomePage));
                    }
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            var shell = Application.Current.MainPage as AppShell;
            if (shell != null)
            {
                shell.Navigation.PopAsync();
            }
        }

        private async void BtnTakePhoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                fileresult = null;
                Stream pickedFileStram = null;
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
                    _viewModel.ImgStream = await fileresult.OpenReadAsync();
                    if (_viewModel.ImgStream != null)
                    {
                        _viewModel.ImageName = fileresult.FileName.ToString();
                        using (MemoryStream memory = new MemoryStream())
                        {
                            Stream stream = _viewModel.ImgStream;
                            stream.CopyTo(memory);
                            imageBytes = memory.ToArray();
                            _viewModel.Image = imageBytes;
                            _viewModel.ImageName = fileresult.FileName.ToString();
                        }
                        Thread.Sleep(2000);
                        ImgPlace.Source = ImageSource.FromStream(() => fileresult.OpenReadAsync().Result);
                        ImgPlace.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                BtnSelectPhoto.Text = "Select";
                await App.Current.MainPage.DisplayAlert("Error", "Error in processing, please try again", "OK");
            }
        }
    }
}