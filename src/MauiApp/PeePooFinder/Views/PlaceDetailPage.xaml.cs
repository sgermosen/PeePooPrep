using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class PlaceDetailPage : ContentPage
{
	private readonly PlaceDetailViewModel _viewModel;
	private bool _appearedOnce;

	public PlaceDetailPage(PlaceDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		// The first load is triggered by the "id" query parameter; later appearances
		// (e.g. back from writing a review) refresh the data.
		if (_appearedOnce)
			await _viewModel.LoadCommand.ExecuteAsync(null);
		_appearedOnce = true;
	}
}
