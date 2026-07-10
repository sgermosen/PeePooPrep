using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class AddReviewPage : ContentPage
{
	public AddReviewPage(AddReviewViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
