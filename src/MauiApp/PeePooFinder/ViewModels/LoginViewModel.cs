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

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public LoginViewModel(IPeePooApi api, ISessionService session, IConnectivity connectivity)
    {
        _api = api;
        _session = session;
        _connectivity = connectivity;
        Title = "Welcome back";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowError("Please enter your email and password.");
            return;
        }

        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await ShowError("No internet connection.");
            return;
        }

        try
        {
            IsBusy = true;
            var auth = await _api.LoginAsync(new LoginRequest { Email = Email.Trim(), Password = Password });
            await _session.SetSessionAsync(auth);
            Password = string.Empty;
            await Shell.Current.GoToAsync("//main");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not reach the server. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static Task GoToRegisterAsync() => Shell.Current.GoToAsync(nameof(RegisterPage));
}
