using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class BlockedUsersPage : ContentPage
{
	private readonly BlockedUsersViewModel _viewModel;

	public BlockedUsersPage(BlockedUsersViewModel viewModel)
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
