using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class PlaceListPage : ContentPage
{
	private readonly PlaceListViewModel _viewModel;

	public PlaceListPage(PlaceListViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadCommand.ExecuteAsync(null);
	}
}
