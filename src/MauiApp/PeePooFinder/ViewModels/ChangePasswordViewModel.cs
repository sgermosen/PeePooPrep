using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;

    [ObservableProperty] private string currentPassword = string.Empty;
    [ObservableProperty] private string newPassword = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;

    public string Hint => RegisterViewModel.PasswordHint;

    public ChangePasswordViewModel(IPeePooApi api, ISessionService session)
    {
        _api = api;
        _session = session;
        Title = "Change password";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
        {
            await ShowError("Fill in your current and new password.");
            return;
        }
        if (NewPassword != ConfirmPassword)
        {
            await ShowError("The new passwords don't match.");
            return;
        }
        if (!RegisterViewModel.PasswordRule.IsMatch(NewPassword))
        {
            await ShowError(Hint);
            return;
        }

        await RunAsync(async () =>
        {
            var auth = await _api.ChangePasswordAsync(CurrentPassword, NewPassword);
            await _session.SetSessionAsync(auth);
            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            await ShowInfo("Password changed", "You're still signed in here. Other devices will need to sign in again.");
            await Shell.Current.GoToAsync("..");
        }, "Couldn't change your password.");
    }
}
