using PeePooFinder.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using PeePooFinder.Models;
using PeePooFinder.Services;
using Acr.UserDialogs;
using Xamarin.Forms.GoogleMaps;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PeePooFinder.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private Pin _selectedPin;

        public ObservableCollection<Pin> Places { get; }
        public Command LoadPlacesCommand { get; }
        public Command AddPinCommand { get; }
        public Command<Pin> PinTappedCommand { get; }
        public HomePageViewModel()
        {
            Title = "Pee Poo Finder";
            Places = new ObservableCollection<Pin>();
            LoadPlacesCommand = new Command(async () => await ExecuteLoadPlacesCommand());

            PinTappedCommand = new Command<Pin>(OnPinSelected);

            AddPinCommand = new Command(OnAddPin);
        }

        async Task ExecuteLoadPlacesCommand()
        {
            IsBusy = true;

            try
            {
                Places.Clear();
                List<Places> places = await MyPlaceService.GetAllPlacesAsync(true);
                foreach (var item in places)
                {
                    if (item.lat > 0 && item.@long > 0)
                    {
                        Places.Add(
                            new Pin
                            {
                                Type = PinType.Place,
                                Label = item.name,
                                Address = item.description,
                                Position = new Position(item.lat, item.@long),                            
                            Tag = "id_"+item.id,
                            }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedPin = null;
        }

        public Pin SelectedPin
        {
            get => _selectedPin;
            set
            {
                SetProperty(ref _selectedPin, value);
                OnPinSelected(value);
            }
        }

        private async void OnAddPin(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnPinSelected(Pin pin)
        {
            if (pin == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            //await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
        private async void OnPinTapped()
        {
            try
            {
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}