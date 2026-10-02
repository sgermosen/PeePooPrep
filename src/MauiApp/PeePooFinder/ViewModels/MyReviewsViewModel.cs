using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PeePooFinder.Models;
using PeePooFinder.Services;
using PeePooFinder.Views;

namespace PeePooFinder.ViewModels;

public partial class MyReviewsViewModel : BaseViewModel
{
    private readonly IPeePooApi _api;

    [ObservableProperty] private ObservableCollection<Review> reviews = new();

    public MyReviewsViewModel(IPeePooApi api)
    {
        _api = api;
        Title = "Your reviews";
    }

    [RelayCommand]
    private Task LoadAsync() => RunAsync(async () =>
        Reviews = new ObservableCollection<Review>(await _api.GetMyReviewsAsync()), "Couldn't load your reviews.");

    [RelayCommand]
    private static Task OpenAsync(Review? review) =>
        review?.PlaceId is null ? Task.CompletedTask : Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={review.PlaceId}");
}
