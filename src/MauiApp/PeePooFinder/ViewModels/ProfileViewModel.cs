using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;

    [ObservableProperty] private string displayName = string.Empty;
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string bio = string.Empty;
    [ObservableProperty] private string? image;

    public ProfileViewModel(IPeePooApi api, ISessionService session)
    {
        _api = api;
        _session = session;
        Title = "Profile";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Username = _session.Username ?? string.Empty;
        DisplayName = _session.DisplayName ?? string.Empty;
        Image = _session.Image;

        if (string.IsNullOrEmpty(Username)) return;

        try
        {
            IsBusy = true;
            var profile = await _api.GetProfileAsync(Username);
            if (profile is not null)
            {
                DisplayName = profile.DisplayName ?? DisplayName;
                Bio = profile.Bio ?? string.Empty;
                Image = profile.Image ?? Image;
            }
        }
        catch
        {
            // keep the cached session values if the profile cannot be refreshed
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            await ShowError("Display name cannot be empty.");
            return;
        }

        try
        {
            IsBusy = true;
            await _api.UpdateProfileAsync(DisplayName.Trim(), Bio?.Trim() ?? string.Empty);
            await ShowInfo("Saved", "Your profile was updated.");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not update your profile.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Sign out", "Are you sure you want to sign out?", "Yes", "Cancel");
        if (!confirm) return;

        await _session.ClearAsync();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Delete account",
            "This permanently deletes your account and your reviews and photos. This cannot be undone.",
            "Delete", "Cancel");
        if (!confirm) return;

        var reallySure = await Shell.Current.DisplayAlert(
            "Are you absolutely sure?",
            "Your account will be deleted for good.",
            "Delete my account", "Keep my account");
        if (!reallySure) return;

        try
        {
            IsBusy = true;
            await _api.DeleteAccountAsync();
            await _session.ClearAsync();
            await Shell.Current.GoToAsync("//login");
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not delete your account. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
