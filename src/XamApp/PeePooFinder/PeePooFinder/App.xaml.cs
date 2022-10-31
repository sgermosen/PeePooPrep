using PeePooFinder.Services;
using PeePooFinder.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PeePooFinder
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            DependencyService.Register<Toast>();
            DependencyService.Register<MockDataStore>();
            DependencyService.Register<LoginService>();
            DependencyService.Register<PlacesService>();
            if (Application.Current.Properties.ContainsKey("UserName"))
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new LoginPage();
            }
        }
        
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
