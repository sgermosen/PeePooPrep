using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

public partial class PlacesViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;
    private readonly List<Place> _allPlaces = new();

    [ObservableProperty]
    private ObservableCollection<Place> places = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isRefreshing;

    public PlacesViewModel(IPeePooApi api)
    {
        _api = api;
        Title = "Nearby spots";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var results = await _api.GetPlacesAsync();
            _allPlaces.Clear();
            _allPlaces.AddRange(results.OrderByDescending(p => p.CreatedAt));
            ApplyFilter();
        }
        catch (ApiException ex)
        {
            await ShowError(ex.Message);
        }
        catch
        {
            await ShowError("Could not load places. Pull to refresh to try again.");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPlaces
            : _allPlaces.Where(p =>
                (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Type?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        Places = new ObservableCollection<Place>(filtered);
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Place? place)
    {
        if (place is null) return;
        await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={place.Id}");
    }

    [RelayCommand]
    private static Task AddPlaceAsync() => Shell.Current.GoToAsync(nameof(SubmitPlacePage));
}
