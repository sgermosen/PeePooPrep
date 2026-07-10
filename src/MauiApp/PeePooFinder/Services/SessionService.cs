using PeePooFinder.Models;

namespace PeePooFinder.Services;

public interface ISessionService
{
    bool IsLoggedIn { get; }
    string? Username { get; }
    string? DisplayName { get; }
    string? Image { get; }
    Task<string?> GetTokenAsync();
    Task SetSessionAsync(AuthResponse auth);
    Task ClearAsync();
}

public class SessionService : ISessionService
{
    private const string TokenKey = "peepoo_token";
    private const string UsernameKey = "peepoo_username";
    private const string DisplayNameKey = "peepoo_displayname";
    private const string ImageKey = "peepoo_image";
    private const string LoggedInKey = "peepoo_logged_in";

    public bool IsLoggedIn => Preferences.Default.Get(LoggedInKey, false);
    public string? Username => Preferences.Default.Get<string?>(UsernameKey, null);
    public string? DisplayName => Preferences.Default.Get<string?>(DisplayNameKey, null);
    public string? Image => Preferences.Default.Get<string?>(ImageKey, null);

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetSessionAsync(AuthResponse auth)
    {
        if (!string.IsNullOrEmpty(auth.Token))
            await SecureStorage.Default.SetAsync(TokenKey, auth.Token);

        Preferences.Default.Set(UsernameKey, auth.Username);
        Preferences.Default.Set(DisplayNameKey, auth.DisplayName);
        Preferences.Default.Set(ImageKey, auth.Image);
        Preferences.Default.Set(LoggedInKey, true);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        Preferences.Default.Remove(UsernameKey);
        Preferences.Default.Remove(DisplayNameKey);
        Preferences.Default.Remove(ImageKey);
        Preferences.Default.Set(LoggedInKey, false);
        return Task.CompletedTask;
    }
}
