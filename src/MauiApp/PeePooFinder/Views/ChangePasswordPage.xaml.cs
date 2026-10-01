using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class ChangePasswordPage : ContentPage
{
	public ChangePasswordPage(ChangePasswordViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
