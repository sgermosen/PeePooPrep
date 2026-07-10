using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder;

public partial class AppShell : Shell
{
	public AppShell(ISessionService session)
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(PlaceDetailPage), typeof(PlaceDetailPage));
		Routing.RegisterRoute(nameof(SubmitPlacePage), typeof(SubmitPlacePage));
		Routing.RegisterRoute(nameof(AddReviewPage), typeof(AddReviewPage));

		CurrentItem = session.IsLoggedIn ? MainTabs : LoginItem;
	}
}
