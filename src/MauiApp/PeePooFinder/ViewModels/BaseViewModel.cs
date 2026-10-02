using CommunityToolkit.Mvvm.ComponentModel;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected static Page? CurrentPage => Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;

    protected static Task ShowError(string message) =>
        CurrentPage?.DisplayAlertAsync("Something's off", message, "OK") ?? Task.CompletedTask;

    protected static Task ShowInfo(string title, string message) =>
        CurrentPage?.DisplayAlertAsync(title, message, "OK") ?? Task.CompletedTask;

    protected static Task<bool> Confirm(string title, string message, string accept, string cancel = "Cancel") =>
        CurrentPage?.DisplayAlertAsync(title, message, accept, cancel) ?? Task.FromResult(false);

    /// <summary>Guests can browse; contributing needs an account. Offers to sign in and returns false if not signed in.</summary>
    protected static async Task<bool> EnsureSignedInAsync(ISessionService session, string toDoWhat)
    {
        if (session.IsLoggedIn) return true;
        if (await Confirm("Sign in", $"You need a free account to {toDoWhat}.", "Sign in", "Not now"))
            await Shell.Current.GoToAsync("//login");
        return false;
    }

    /// <summary>Runs an action with the busy flag, showing server messages and a fallback for anything else.</summary>
    protected async Task RunAsync(Func<Task> action, string fallbackError, bool showBusy = true)
    {
        if (showBusy && IsBusy) return;
        try
        {
            if (showBusy) IsBusy = true;
            await action();
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            // AuthMessageHandler already ended the session and the shell is taking the user to sign in.
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch (HttpRequestException)
        {
            await ShowError("Can't reach PeePoo right now. Check your connection and try again.");
        }
        catch (TaskCanceledException)
        {
            await ShowError("The server took too long to answer. Please try again.");
        }
        catch (Exception)
        {
            await ShowError(fallbackError);
        }
        finally
        {
            if (showBusy) IsBusy = false;
        }
    }

    protected static async Task<string?> AskReportReason()
    {
        var page = CurrentPage;
        if (page is null) return null;
        var reason = await page.DisplayActionSheetAsync("What's wrong?", "Cancel", null,
            "Doesn't exist or is closed for good", "Wrong information", "Offensive or inappropriate", "Spam", "Other");
        if (reason is null || reason == "Cancel") return null;
        if (reason != "Other") return reason;

        var custom = await page.DisplayPromptAsync("Report", "Tell us what's wrong.", "Send", "Cancel", maxLength: 500);
        return string.IsNullOrWhiteSpace(custom) ? null : custom.Trim();
    }
}
