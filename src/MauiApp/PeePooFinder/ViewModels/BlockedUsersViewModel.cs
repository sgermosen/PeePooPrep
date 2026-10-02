using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Services;

namespace PeePooFinder.ViewModels;

public partial class BlockedUsersViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;

    [ObservableProperty] private ObservableCollection<string> usernames = new();

    public BlockedUsersViewModel(IPeePooApi api)
    {
        _api = api;
        Title = "Blocked people";
    }

    [RelayCommand]
    private Task LoadAsync() => RunAsync(async () =>
        Usernames = new ObservableCollection<string>(await _api.GetBlockedUsersAsync()), "Couldn't load the list.");

    [RelayCommand]
    private async Task UnblockAsync(string? username)
    {
        if (string.IsNullOrEmpty(username)) return;
        if (!await Confirm("Unblock", $"See reviews from @{username} again?", "Unblock")) return;
        await RunAsync(async () =>
        {
            await _api.UnblockUserAsync(username);
            Usernames.Remove(username);
        }, "Couldn't unblock this person.", showBusy: false);
    }
}
