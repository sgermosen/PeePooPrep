using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Newtonsoft.Json;
namespace PeePooFinder.ViewModels
{
    [QueryProperty(nameof(PlaceDetail), nameof(PlaceDetail))]
    public class SubmitPlaceViewModel : BaseViewModel
    {
        string placeDetail = "";
        public Places place;
        public string PlaceDetail
        {
            get { return placeDetail; }
            set
            {
                placeDetail = value;
                SetPlaceData(placeDetail);

            }
        }


        public Places Place
        {
            get { return place; }
            set
            {
                place = value;
                OnPropertyChanged("PlaceDetail");
            }
        }
        private async void SetPlaceData(string placeDetail)
        {
            try
            {

                Place = JsonConvert.DeserializeObject<Places>(placeDetail);
                Console.WriteLine(Place.lat);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
;