#if DEBUG
using System;
using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;
using PeePooFinder.DataSettings;

[Application(Debuggable = true)]
#else
using System;
using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;
using PeePooFinder.DataSettings;


[Application(Debuggable = true)]
#endif
[MetaData("com.google.android.maps.v2.API_KEY",
			  Value = GMapsKey.GoogleMapsApiKey)]
public class MainApplication : Application
{
	public MainApplication(IntPtr handle, JniHandleOwnership transer)
	  : base(handle, transer)
	{
	}

	public override void OnCreate()
	{
		base.OnCreate();
		CrossCurrentActivity.Current.Init(this);
	}
}