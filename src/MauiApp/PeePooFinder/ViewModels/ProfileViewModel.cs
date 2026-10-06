using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly ISessionService _session;
    private readonly AppSettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGuest))]
    private bool isSignedIn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initial))]
    private string displayName = string.Empty;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string bio = string.Empty;
    [ObservableProperty] private string statsLabel = string.Empty;

    public bool IsGuest => !IsSignedIn;
    public string Initial => string.IsNullOrWhiteSpace(DisplayName) ? "?" : DisplayName.Trim()[..1].ToUpperInvariant();
    public string AppVersion => $"Version {AppInfo.Current.VersionString}";

    public ProfileViewModel(IPeePooApi api, ISessionService session, AppSettings settings)
    {
        _api = api;
        _session = session;
        _settings = settings;
        Title = "You";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsSignedIn = _session.IsLoggedIn;
        if (!IsSignedIn) return;

        Username = _session.Username ?? string.Empty;
        DisplayName = _session.DisplayName ?? string.Empty;
        if (string.IsNullOrEmpty(Username)) return;

        try
        {
            var profile = await _api.GetProfileAsync(Username);
            if (profile is null) return;
            DisplayName = profile.DisplayName ?? DisplayName;
            Bio = profile.Bio ?? string.Empty;
            StatsLabel = $"{Plural(profile.PlacesCount, "place")} added · {Plural(profile.ReviewsCount, "review")} · joined {profile.JoinedAt.ToLocalTime():MMM yyyy}";
        }
        catch
        {
            // keep cached values when offline
        }
    }

    private static string Plural(int n, string word) => n == 1 ? $"1 {word}" : $"{n} {word}s";

    [RelayCommand]
    private static Task SignInAsync() => Shell.Current.GoToAsync("//login");

    [RelayCommand]
    private static async Task CreateAccountAsync()
    {
        await Shell.Current.GoToAsync("//login");
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            await ShowError("Your display name can't be empty.");
            return;
        }

        await RunAsync(async () =>
        {
            await _api.UpdateProfileAsync(DisplayName.Trim(), Bio?.Trim() ?? string.Empty);
            _session.UpdateDisplayName(DisplayName.Trim());
            await ShowInfo("Saved", "Your profile was updated.");
        }, "Couldn't update your profile.");
    }

    [RelayCommand] private static Task OpenSavedAsync() => Shell.Current.GoToAsync($"{nameof(PlaceListPage)}?mode=saved");
    [RelayCommand] private static Task OpenMinePlacesAsync() => Shell.Current.GoToAsync($"{nameof(PlaceListPage)}?mode=mine");
    [RelayCommand] private static Task OpenMyReviewsAsync() => Shell.Current.GoToAsync(nameof(MyReviewsPage));
    [RelayCommand] private static Task OpenChangePasswordAsync() => Shell.Current.GoToAsync(nameof(ChangePasswordPage));
    [RelayCommand] private static Task OpenBlockedAsync() => Shell.Current.GoToAsync(nameof(BlockedUsersPage));

    [RelayCommand] private Task OpenHelpAsync() => OpenWebAsync("ayuda");
    [RelayCommand] private Task OpenPrivacyAsync() => OpenWebAsync("privacy");
    [RelayCommand] private Task OpenTermsAsync() => OpenWebAsync("terms");

    private async Task OpenWebAsync(string path)
    {
        try
        {
            await Browser.Default.OpenAsync(_settings.Api.WebUrl(path), BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            await ShowError("Couldn't open the browser.");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (!await Confirm("Sign out", "Sign out of PeePoo Finder on this device?", "Sign out")) return;
        await _session.ClearAsync();
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        if (!await Confirm("Delete account", "This permanently deletes your account, your reviews and your photos. This can't be undone.", "Delete"))
            return;
        if (!await Confirm("Are you absolutely sure?", "Your account will be deleted for good.", "Delete my account", "Keep my account"))
            return;

        await RunAsync(async () =>
        {
            await _api.DeleteAccountAsync();
            await _session.ClearAsync();
            await LoadAsync();
            await ShowInfo("Account deleted", "Your account and your data were deleted.");
        }, "Couldn't delete your account. Please try again.");
    }
}
