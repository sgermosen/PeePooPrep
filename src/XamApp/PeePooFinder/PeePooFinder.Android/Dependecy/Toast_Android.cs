using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PeePooFinder.Droid.Dependecy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toast = PeePooFinder.Services.Toast;

[assembly: Xamarin.Forms.Dependency(typeof(Toast_Android))]
namespace PeePooFinder.Droid.Dependecy
{
    public class Toast_Android : Toast
    {
        public void Show(string message)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
    }
}