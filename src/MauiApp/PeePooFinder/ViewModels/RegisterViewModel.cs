using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public RegisterViewModel(IPeePooApi api, ISessionService session)
    {
        _api = api;
        _session = session;
        Title = "Create account";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowError("Please fill in every field.");
            return;
        }

        if (Password.Length < 8)
        {
            await ShowError("Password must be at least 8 characters and include an uppercase letter, a lowercase letter and a digit.");
            return;
        }

        try
        {
            IsBusy = true;
            var auth = await _api.RegisterAsync(new RegisterRequest
            {
                DisplayName = DisplayName.Trim(),
                Username = Username.Trim(),
                Email = Email.Trim(),
                Password = Password
            });
            await _session.SetSessionAsync(auth);
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
    private static Task BackToLoginAsync() => Shell.Current.GoToAsync("..");
}
