using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    // Same rules the server enforces, so mistakes show up before sending.
    internal static readonly Regex UsernameRule = new("^[a-zA-Z0-9_.]{3,24}$");
    internal static readonly Regex PasswordRule = new("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$");
    internal const string PasswordHint = "At least 8 characters, with an uppercase letter, a lowercase letter and a number.";

    private readonly IPeePooApi _api;
    private readonly ISessionService _session;

    [ObservableProperty] private string displayName = string.Empty;
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    public RegisterViewModel(IPeePooApi api, ISessionService session)
    {
        _api = api;
        _session = session;
        Title = "Create account";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await ShowError("Please fill in every field.");
            return;
        }

        if (!UsernameRule.IsMatch(Username.Trim()))
        {
            await ShowError("Usernames are 3–24 characters: letters, numbers, dots or underscores.");
            return;
        }

        if (!PasswordRule.IsMatch(Password))
        {
            await ShowError(PasswordHint);
            return;
        }

        await RunAsync(async () =>
        {
            var auth = await _api.RegisterAsync(new RegisterRequest
            {
                DisplayName = DisplayName.Trim(),
                Username = Username.Trim(),
                Email = Email.Trim(),
                Password = Password
            });
            await _session.SetSessionAsync(auth);
            await Shell.Current.GoToAsync("//main");
        }, "Couldn't create your account. Please try again.");
    }

    [RelayCommand]
    private static Task BackToLoginAsync() => Shell.Current.GoToAsync("..");
}
