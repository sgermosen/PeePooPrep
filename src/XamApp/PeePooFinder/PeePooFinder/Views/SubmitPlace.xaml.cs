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

namespace PeePooFinder.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubmitPlace : ContentPage
    {
        private FileResult fileresult { get; set; }
        private Stream pickedFileStream { get; set; }
       private byte[] imageBytes { get; set; }
        PlacesViewModel _viewModel;
        public SubmitPlace()
        {
            InitializeComponent();
            //imgProfile.Source = "LogoTransparent.png";
            //this.BindingContext = new PlacesViewModel();
            this.BindingContext = _viewModel = new PlacesViewModel();
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

                // BtnSelectPhoto.Text = "Select";
                await App.Current.MainPage.DisplayAlert("Error", "Error in processing, please try again", "OK");
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