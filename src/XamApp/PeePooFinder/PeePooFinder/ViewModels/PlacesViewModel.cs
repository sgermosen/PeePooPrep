using Acr.UserDialogs;
using Newtonsoft.Json;
using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Toast = PeePooFinder.Services.Toast;

namespace PeePooFinder.ViewModels
{
    [QueryProperty(nameof(PlaceDetail), nameof(PlaceDetail))]
    public class PlacesViewModel : BaseViewModel
    {
        public PlacesViewModel()
        {
            SubmitPlaceCommand = new Command(OnSubmitPlace);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SubmitPlaceCommand.ChangeCanExecute();
            BindPlaceType();
        }
        public Command SubmitPlaceCommand { get; }
        public Command CancelCommand { get; }

        List<PlaceTypes> placeTypesList;
        public List<PlaceTypes> PlaceTypeList
        {
            get { return placeTypesList; }
            set => SetProperty(ref placeTypesList, value);
        }

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

        private string ownerUserName;
        public string OwnerUserName
        {
            get => ownerUserName;
            set => SetProperty(ref ownerUserName, value);
        }
        private int rating;
        public int Rating
        {
            get => rating;
            set => SetProperty(ref rating, value);
        }

        private double @long;
        public double @Long
        {
            get => @long;
            set => SetProperty(ref @long, value);
        }


        private double lat;
        public double Lat
        {
            get => lat;
            set => SetProperty(ref lat, value);
        }


        private bool isAproved;
        public bool IsAproved
        {
            get => isAproved;
            set => SetProperty(ref isAproved, value);
        }
        private PlaceTypes _selectedType;
        public PlaceTypes SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                SetProperty(ref _selectedType, value);
                //put here your code  
                Type = _selectedType.PlaceType;
            }
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
        private Stream imgStream;
        public Stream ImgStream
        {
            get => imgStream;
            set => SetProperty(ref imgStream, value);
        }

        private byte[] image;
        public byte[] Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }
        private string imageName;
        public string ImageName
        {
            get => imageName;
            set => SetProperty(ref imageName, value);
        }


        private async void SetPlaceData(string placeDetail)
        {
            try
            {
                Place = JsonConvert.DeserializeObject<Places>(placeDetail);
                Lat = place.lat;
                @long = place.@long;
                Console.WriteLine(Place.lat);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void BindPlaceType()
        {
            List<PlaceTypes> lst = new List<PlaceTypes>();
            lst.Add(new PlaceTypes { PlaceType = "Familiar", PlaceTypeName = "Familiar" });
            lst.Add(new PlaceTypes { PlaceType = "Individual", PlaceTypeName = "Individual" });
            PlaceTypeList = lst;
        }

        private async void OnSubmitPlace()
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(Name)
                    || string.IsNullOrEmpty(Description)
                       || string.IsNullOrEmpty(Type)
                       || string.IsNullOrEmpty(observations)
                    )
                {
                    msg = "Please fill all the details and try again";
                    DependencyService.Get<Toast>().Show(msg);
                }
                else if (string.IsNullOrEmpty(ImageName))
                {
                    DependencyService.Get<Toast>().Show("Please take an image before submit");
                }
                else
                {
                    using (UserDialogs.Instance.Loading("Saving Place data..."))
                    {
                        SubmitModel objSumit = new SubmitModel
                        {
                            id = Guid.NewGuid().ToString(),
                            name = Name,
                            description = Description,
                            type = Type,
                            createdAt = System.DateTime.Now,
                            rating = Rating,
                            urinals = Urinals,
                            toilets = Toilets,
                            haveBabyChanger = HaveBabyChanger,
                            isRoomy = IsRoomy,
                            isAvailable = isAvailable,
                            observations = Observations,
                            ImageName = ImageName,
                            ImageBytes = Image,
                            ImgStream = imgStream,
                            lat = Lat,
                            @long = @Long,
                            OwnerUserName = OwnerUserName
                        };
                        var objGetPlaceDetails = await MyPlaceService.SubmitPlace(objSumit);
                        if (objGetPlaceDetails.Status == "success")
                        {
                            DependencyService.Get<Toast>().Show(objGetPlaceDetails.Message);
                            Thread.Sleep(1500);
                            Application.Current.MainPage = new AppShell();
                            //await Shell.Current.Navigation.PopToRootAsync();
                        }
                        else
                        {
                            DependencyService.Get<Toast>().Show(objGetPlaceDetails.Message);
                        }
                        //await Shell.Current.GoToAsync(nameof(HomePage));
                    }
                }
            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
            }
        }

        private async void OnCancel()
        {
            Application.Current.MainPage = new AppShell();
            //await Shell.Current.Navigation.PopToRootAsync();
            //await Shell.Current.GoToAsync(nameof(HomePage));
        }
    }
}