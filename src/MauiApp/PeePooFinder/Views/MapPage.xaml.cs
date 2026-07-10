using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
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
				Address = place.Type,
				Location = new Location(place.Lat, place.Long),
				Type = PinType.Place
			};
			pin.InfoWindowClicked += async (_, _) =>
				await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}?id={placeId}");
			RestroomMap.Pins.Add(pin);
		}

		if (_viewModel.Region is not null)
			RestroomMap.MoveToRegion(_viewModel.Region);
	}
}
