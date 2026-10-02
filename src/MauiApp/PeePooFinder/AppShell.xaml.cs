using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder;

public partial class AppShell : Shell
{
    private bool _showingExpiry;

    public AppShell(ISessionService session)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(PlaceDetailPage), typeof(PlaceDetailPage));
        Routing.RegisterRoute(nameof(SubmitPlacePage), typeof(SubmitPlacePage));
        Routing.RegisterRoute(nameof(AddReviewPage), typeof(AddReviewPage));
        Routing.RegisterRoute(nameof(PlaceListPage), typeof(PlaceListPage));
        Routing.RegisterRoute(nameof(MyReviewsPage), typeof(MyReviewsPage));
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(BlockedUsersPage), typeof(BlockedUsersPage));

        // Anyone can browse; signing in is only needed to contribute.
        CurrentItem = MainTabs;

        session.SessionExpired += (_, _) => MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_showingExpiry) return;
            _showingExpiry = true;
            try
            {
                await DisplayAlertAsync("Signed out", "Your session ended (for example, the password was changed). Please sign in again.", "OK");
                await GoToAsync("//login");
            }
            finally
            {
                _showingExpiry = false;
            }
        });
    }
}
