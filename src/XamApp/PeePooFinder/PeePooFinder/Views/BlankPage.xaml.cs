using PeePooFinder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Toast = PeePooFinder.Services.Toast;
namespace PeePooFinder.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlankPage : ContentPage
    {
        public BlankPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
            DependencyService.Get<Toast>().Show("WELCOME");
        }
    }
}