using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class MyReviewsPage : ContentPage
{
	private readonly MyReviewsViewModel _viewModel;

	public MyReviewsPage(MyReviewsViewModel viewModel)
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
