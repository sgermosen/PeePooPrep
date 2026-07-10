using PeePooFinder.ViewModels;

namespace PeePooFinder.Views;

public partial class PlaceDetailPage : ContentPage
{
	public PlaceDetailPage(PlaceDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
