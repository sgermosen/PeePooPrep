using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeePooFinder.Services;
using PeePooFinder.ViewModels;
using PeePooFinder.Views;

namespace PeePooFinder;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		using var stream = Assembly.GetExecutingAssembly()
			.GetManifestResourceStream("PeePooFinder.appsettings.json");
		if (stream is not null)
		{
			var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
			builder.Configuration.AddConfiguration(config);
		}

		var settings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();
		builder.Services.AddSingleton(settings);

		builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddSingleton(Connectivity.Current);
		builder.Services.AddSingleton(Geolocation.Default);
		builder.Services.AddSingleton(MediaPicker.Default);

		builder.Services.AddTransient<AuthMessageHandler>();
		builder.Services.AddHttpClient<IPeePooApi, PeePooApiClient>(client =>
		{
			client.BaseAddress = new Uri(settings.Api.BaseUrl);
		}).AddHttpMessageHandler<AuthMessageHandler>();

		RegisterViewModelsAndPages(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void RegisterViewModelsAndPages(IServiceCollection services)
	{
		services.AddSingleton<AppShell>();
		services.AddTransient<LoginViewModel>();
		services.AddTransient<LoginPage>();
		services.AddTransient<RegisterViewModel>();
		services.AddTransient<RegisterPage>();
		services.AddTransient<PlacesViewModel>();
		services.AddTransient<PlacesPage>();
		services.AddTransient<MapViewModel>();
		services.AddTransient<MapPage>();
		services.AddTransient<PlaceDetailViewModel>();
		services.AddTransient<PlaceDetailPage>();
		services.AddTransient<SubmitPlaceViewModel>();
		services.AddTransient<SubmitPlacePage>();
		services.AddTransient<AddReviewViewModel>();
		services.AddTransient<AddReviewPage>();
		services.AddTransient<ProfileViewModel>();
		services.AddTransient<ProfilePage>();
	}
}
