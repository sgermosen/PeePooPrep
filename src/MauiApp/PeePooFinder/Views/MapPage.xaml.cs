using Microsoft.Maui.Controls.Maps;
using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class MapPage : ContentPage
{
	private readonly MapViewModel _viewModel;

	public MapPage(MapViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadCommand.ExecuteAsync(null);

		RestroomMap.Pins.Clear();
		foreach (var place in _viewModel.Places)
		{
			var placeId = place.Id;
			var pin = new Pin
			{
				Label = place.Name ?? "Restroom",
				Address = place.HasReviews ? $"{place.TypeLabel} · ★ {place.ScoreLabel}" : place.TypeLabel,
				Location = new Location(place.Lat, place.Long),
				Type = PinType.Place
			};
			pin.InfoWindowClicked += async (_, _) =>
				await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={placeId}");
			RestroomMap.Pins.Add(pin);
		}

		RestroomMap.MoveToRegion(_viewModel.Region);
	}
}
