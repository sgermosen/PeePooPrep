using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace PeePooFinder.ViewModels
{
    [QueryProperty(nameof(PlaceID), nameof(PlaceID))]
    public class PlaceDetailViewModel : BaseViewModel
    {
        private bool _isConnected;
        private string _placeID;
        private Places _placedetail;
        public PlaceDetailViewModel()
        {

            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => CancelCommand.ChangeCanExecute();
            IsConnected = CheckConnectivity();
          //  LoadAllPlaceDetails();
            //LoginCommand = new Command(OnLogin);
        }
        public Command CancelCommand;

        private List<double> lstLatLong;
        public List<double> LstLatLong
        {
            get => lstLatLong;
            set => SetProperty(ref lstLatLong, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }


        private string observations;
        public string Observations
        {
            get => observations;
            set => SetProperty(ref observations, value);
        }

        private bool isAvailable;
        public bool IsAvailable
        {
            get => isAvailable;
            set => SetProperty(ref isAvailable, value);
        }


        private bool haveBabyChanger;
        public bool HaveBabyChanger
        {
            get => haveBabyChanger;
            set => SetProperty(ref haveBabyChanger, value);
        }


        private bool isRoomy;
        public bool IsRoomy
        {
            get => isRoomy;
            set => SetProperty(ref isRoomy, value);
        }

        private int urinals;
        public int Urinals
        {
            get => urinals;
            set => SetProperty(ref urinals, value);
        }

        private int toilets;
        public int Toilets
        {
            get => toilets;
            set => SetProperty(ref toilets, value);
        }


        private int rating;
        public int Rating
        {
            get => rating;
            set => SetProperty(ref rating, value);
        }

        private double longitude;
        public double Longitude
        {
            get => longitude;
            set => SetProperty(ref longitude, value);
        }


        private double lat;
        public double Lat
        {
            get => lat;
            set => SetProperty(ref lat, value);
        }

        private string ownerUserName;
        public string OwnerUserName {
            get => ownerUserName;
            set => SetProperty(ref ownerUserName, value);
        }
        private bool isAproved;
        public bool IsAproved
        {
            get => isAproved;
            set => SetProperty(ref isAproved, value);
        }

        private string type;
        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }
        private string ratingstart;
        public string RatingStar
        {
            get => ratingstart;
            set => SetProperty(ref ratingstart, value);
        }


        private string image;
        public string Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        private bool forceRefresh;
        public bool ForceRefresh
        {
            get => forceRefresh;
            set => SetProperty(ref forceRefresh, value);
        }



        public string PlaceID
        {
            get
            {
                return _placeID;
            }
            set
            {
                _placeID = value;
                LoadPlaceDetail(value);
            }
        }

        public bool IsConnected { 
        get => _isConnected;
            set { 
            _isConnected= value;
                OnPropertyChanged("IsConnected");
            }
        }
        public Places PlaceDetail
        {
            get => _placedetail;
            set
            {
                _placedetail = value;
                OnPropertyChanged("PlaceDetail");
            }
        }
        public async void LoadPlaceDetail(string placeid)
        {
            try
            {
                PlaceDetail = await MyPlaceService.GetSinglePlaceAsync(placeid);
                if (PlaceDetail != null)
                {
                    PlaceDetail.visits = await MyPlaceService.GetVisitsByPlaceID(placeid);
                    Name = PlaceDetail.name;
                    Description = PlaceDetail.description;
                    Observations = PlaceDetail.observations;
                    IsAvailable = PlaceDetail.isAvailable;
                    HaveBabyChanger = PlaceDetail.haveBabyChanger;
                    IsRoomy = PlaceDetail.isRoomy;
                    Urinals = PlaceDetail.urinals;
                    Toilets = PlaceDetail.toilets;
                    Rating = PlaceDetail.rating;
                    Longitude = PlaceDetail.@long;
                    Lat = PlaceDetail.lat;
                    IsAproved = PlaceDetail.isAproved;
                    Type = PlaceDetail.type;
                    OwnerUserName = PlaceDetail.ownerUsername;
                    if (PlaceDetail.image != null)
                    {
                        Image = PlaceDetail.image;
                    }
                    else
                    {
                        Image = "LogoTransparent.png";
                    }
                    for(int i=0; i< Rating; i++)
                    {
                        RatingStar = "StarYellow.png";
                    }
                }
               
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
        public async void GetPlaceLatLong(string placeid)
        {
            LstLatLong = new List<double>();
            try
            {
                PlaceDetail = await MyPlaceService.GetSinglePlaceAsync(placeid);
                if (PlaceDetail != null)
                {
                    LstLatLong.Add(PlaceDetail.lat);
                    LstLatLong.Add(PlaceDetail.@long);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
        public async void LoadAllPlaceDetails()
            {
            List<Places> objGetPlaceDetails = null;
            objGetPlaceDetails = await MyPlaceService.GetAllPlacesAsync();
            if (objGetPlaceDetails != null)
            {
                Name = objGetPlaceDetails[0].name;
                Description = objGetPlaceDetails[0].description;
                Observations = objGetPlaceDetails[0].observations;
                IsAvailable = objGetPlaceDetails[0].isAvailable;
                HaveBabyChanger = objGetPlaceDetails[0].haveBabyChanger;
                IsRoomy = objGetPlaceDetails[0].isRoomy;
                Urinals = objGetPlaceDetails[0].urinals;
                Toilets = objGetPlaceDetails[0].toilets;
                Rating = objGetPlaceDetails[0].rating;

                Longitude = objGetPlaceDetails[0].@long;
                Lat = objGetPlaceDetails[0].lat;
                IsAproved = objGetPlaceDetails[0].isAproved;
                Type = objGetPlaceDetails[0].type;
                if (objGetPlaceDetails[0].image != null)
                {
                    Image = objGetPlaceDetails[0].image;
                }
                else
                {
                    Image = "LogoTransparent.png";
                }
                for (int i = 0; i < Rating; i++)
                {
                    RatingStar = "StarYellow.png";
                }
            }
        }



        private bool CheckConnectivity()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }
        private void OnCancel()
        { 
        }
    }
}
