using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class SubmitPlacePage : ContentPage
{
	private readonly SubmitPlaceViewModel _viewModel;

	public SubmitPlacePage(SubmitPlaceViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (_viewModel.LocateCommand.CanExecute(null))
			_viewModel.LocateCommand.Execute(null);
	}
}
