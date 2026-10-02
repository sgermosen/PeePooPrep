using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class PlacesPage : ContentPage
{
	private readonly PlacesViewModel _viewModel;

	public PlacesPage(PlacesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadIfNeededAsync();
	}
}
