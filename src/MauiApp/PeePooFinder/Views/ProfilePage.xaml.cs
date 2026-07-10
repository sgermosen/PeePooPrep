using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class ProfilePage : ContentPage
{
	private readonly ProfileViewModel _viewModel;

	public ProfilePage(ProfileViewModel viewModel)
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
