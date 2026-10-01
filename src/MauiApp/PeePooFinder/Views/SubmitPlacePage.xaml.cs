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

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (!_viewModel.HasLocation)
			await _viewModel.LocateCommand.ExecuteAsync(null);
	}
}
