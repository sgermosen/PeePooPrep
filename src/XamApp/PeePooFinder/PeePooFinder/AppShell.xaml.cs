using PeePooFinder.ViewModels;
using PeePooFinder.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PeePooFinder
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(SubmitPlace), typeof(SubmitPlace));
            Routing.RegisterRoute(nameof(CommentsPage), typeof(CommentsPage));
            Routing.RegisterRoute(nameof(PlaceDetails), typeof(PlaceDetails));
            Routing.RegisterRoute(nameof(UserPage), typeof(UserPage));
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            
        }

    }
}
