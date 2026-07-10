using CommunityToolkit.Mvvm.ComponentModel;

namespace PeePooFinder.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected static Task ShowError(string message) =>
        Shell.Current?.DisplayAlert("Oops", message, "OK") ?? Task.CompletedTask;

    protected static Task ShowInfo(string title, string message) =>
        Shell.Current?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;
}
