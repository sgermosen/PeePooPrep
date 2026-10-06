using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

/// <summary>"Saved" places or "Places you added", depending on the mode query parameter.</summary>
[QueryProperty(nameof(Mode), "mode")]
public partial class PlaceListViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;

    [ObservableProperty] private string mode = "saved";
    [ObservableProperty] private ObservableCollection<Place> places = new();
    [ObservableProperty] private string emptyText = string.Empty;

    public PlaceListViewModel(IPeePooApi api)
    {
        _api = api;
    }

    partial void OnModeChanged(string value)
    {
        Title = value == "mine" ? "Places you added" : "Saved places";
        EmptyText = value == "mine"
            ? "You haven't added any places yet. Use + on the Nearby tab when you find one."
            : "Nothing saved yet. Tap Save on a place to keep it here.";
    }

    [RelayCommand]
    private Task LoadAsync() => RunAsync(async () =>
    {
        var list = Mode == "mine" ? await _api.GetMyPlacesAsync() : await _api.GetSavedPlacesAsync();
        Places = new ObservableCollection<Place>(list);
    }, "Couldn't load your places.");

    [RelayCommand]
    private static Task GoToDetailAsync(Place? place) =>
        place is null ? Task.CompletedTask : Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={place.Id}");
}
