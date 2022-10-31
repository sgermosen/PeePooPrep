using PeePooFinder.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace PeePooFinder.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}