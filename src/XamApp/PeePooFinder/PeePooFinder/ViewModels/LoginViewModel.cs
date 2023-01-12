using Acr.UserDialogs;
using PeePooFinder.Models;
using PeePooFinder.Services;
using System;
using Xamarin.Forms;

namespace PeePooFinder.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string emailID;
        private string pass;
        private bool _isConnected;
        public LoginViewModel()
        {

            LoginCommand = new Command(OnLogin, ValidateLogin);
            this.PropertyChanged +=
                (_, __) => LoginCommand.ChangeCanExecute();
            _isConnected = CheckConnectivity();
            LoginCommand = new Command(OnLogin);
        }
        public string UserEmailID
        {
            get => emailID;
            set => SetProperty(ref emailID, value);
        }
        public bool IsConnectedToInternet
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string UserPassword
        {
            get => pass;
            set => SetProperty(ref pass, value);
        }
        private bool ValidateLogin()
        {
            return !String.IsNullOrWhiteSpace(emailID)
                && !String.IsNullOrWhiteSpace(pass);
        }
        public Command LoginCommand { get; }

        private async void OnLogin()
        {
            try
            {

                if (IsConnectedToInternet || CheckConnectivity())
                {
                    using (UserDialogs.Instance.Loading("Loggin In..."))
                    {
                        LoginModel userMoel = new LoginModel()
                        {
                            Email = UserEmailID,
                            Password = UserPassword,
                        };
                        LoginResponseModel objResponse = await MyLoginService.PerformLogin(userMoel);
                        if (objResponse != null)
                        {
                            Application.Current.Properties.Add("UserName", objResponse.username);
                            Application.Current.Properties.Add("Token", objResponse.token);
                            Application.Current.Properties.Add("displayName", objResponse.displayName);
                            Application.Current.Properties.Add("Image", objResponse.image);

                            //var locator = CrossGeolocator.Current;
                            //locator.DesiredAccuracy = 50;
                            //var position = await locator.GetLastKnownLocationAsync();
                            //Console.WriteLine("Position Status: {0}", position.Timestamp);
                            //Console.WriteLine("Position Latitude: {0}", position.Latitude);
                            //Console.WriteLine("Position Longitude: {0}", position.Longitude);

                            Application.Current.MainPage = new AppShell();
                        }
                        else
                        {
                            DependencyService.Get<Toast>().Show("Please enter valid Email ID and Password");
                        }
                    }
                }
                else
                {
                    DependencyService.Get<Toast>().Show("Please check your internet connection and try again..");
                }
                // This will pop the current page off the navigation stack
                // await Shell.Current.GoToAsync($"//{nameof(AppShell)}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool CheckConnectivity()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }
    }
}
