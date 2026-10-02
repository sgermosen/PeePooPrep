using PeePooFinder.Models;

namespace PeePooFinder.Services;

public interface ISessionService
{
    bool IsLoggedIn { get; }
    string? Username { get; }
    string? DisplayName { get; }
    Task<string?> GetTokenAsync();
    Task SetSessionAsync(AuthResponse auth);
    void UpdateDisplayName(string displayName);
    Task ClearAsync();

    /// <summary>Raised when the server rejects the stored token (password changed elsewhere, account removed, expired).</summary>
    event EventHandler? SessionExpired;
    void NotifyExpired();
}

public class SessionService : ISessionService
{
    private const string TokenKey = "peepoo_token";
    private const string UsernameKey = "peepoo_username";
    private const string DisplayNameKey = "peepoo_displayname";
    private const string LoggedInKey = "peepoo_logged_in";

    public event EventHandler? SessionExpired;

    public bool IsLoggedIn => Preferences.Default.Get(LoggedInKey, false);
    public string? Username => Preferences.Default.Get<string?>(UsernameKey, null);
    public string? DisplayName => Preferences.Default.Get<string?>(DisplayNameKey, null);

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch
        {
            // SecureStorage can fail after an OS-level restore; treat it as signed out.
            return null;
        }
    }

    public async Task SetSessionAsync(AuthResponse auth)
    {
        if (!string.IsNullOrEmpty(auth.Token))
            await SecureStorage.Default.SetAsync(TokenKey, auth.Token);

        Preferences.Default.Set(UsernameKey, auth.Username);
        Preferences.Default.Set(DisplayNameKey, auth.DisplayName);
        Preferences.Default.Set(LoggedInKey, true);
    }

    public void UpdateDisplayName(string displayName) => Preferences.Default.Set(DisplayNameKey, displayName);

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        Preferences.Default.Remove(UsernameKey);
        Preferences.Default.Remove(DisplayNameKey);
        Preferences.Default.Set(LoggedInKey, false);
        return Task.CompletedTask;
    }

    public void NotifyExpired() => SessionExpired?.Invoke(this, EventArgs.Empty);
}
