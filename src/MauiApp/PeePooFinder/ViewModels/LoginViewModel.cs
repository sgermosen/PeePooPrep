using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;
    private readonly IConnectivity _connectivity;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public LoginViewModel(IPeePooApi api, ISessionService session, IConnectivity connectivity)
    {
        _api = api;
        _session = session;
        _connectivity = connectivity;
        Title = "Sign in";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowError("Enter your email and password.");
            return;
        }

        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await ShowError("You're offline. Connect to the internet and try again.");
            return;
        }

        await RunAsync(async () =>
        {
            var auth = await _api.LoginAsync(new LoginRequest { Email = Email.Trim(), Password = Password });
            await _session.SetSessionAsync(auth);
            Password = string.Empty;
            await Shell.Current.GoToAsync("//main");
        }, "Couldn't sign you in. Please try again.");
    }

    [RelayCommand]
    private static Task GoToRegisterAsync() => Shell.Current.GoToAsync(nameof(RegisterPage));

    [RelayCommand]
    private static Task ContinueAsGuestAsync() => Shell.Current.GoToAsync("//main");
}
