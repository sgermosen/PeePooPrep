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

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (_viewModel.LoadCommand.CanExecute(null))
			_viewModel.LoadCommand.Execute(null);
	}
}
