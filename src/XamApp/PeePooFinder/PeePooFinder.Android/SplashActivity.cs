using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Airbnb.Lottie;
using Android.Animation;

namespace PeePooFinder.Droid
{

    [Activity(Theme = "@style/Theme.Splash", Icon = "@drawable/Logo", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity, Animator.IAnimatorListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.splash_layout);
            // Create your application here
            var animationview = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            animationview.AddAnimatorListener(this);
        }

        public void animationCancel(Animator animation)
        {

        }
        public void OnAnimationEnd(Animator animation)
        {
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        public void OnAnimationCancel(Animator animation)
        {

        }

        public void OnAnimationRepeat(Animator animation)
        {

        }

        public void OnAnimationStart(Animator animation)
        {
            //throw new NotImplementedException();
        }
    }
}