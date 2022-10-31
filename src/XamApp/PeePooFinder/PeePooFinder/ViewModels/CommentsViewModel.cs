using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Toast = PeePooFinder.Services.Toast;
using Xamarin.Forms;
using Acr.UserDialogs;
using System.Threading;
using System.IO;

namespace PeePooFinder.ViewModels
{
    [QueryProperty(nameof(CurPlaceID), nameof(CurPlaceID))]
    public class CommentsViewModel : BaseViewModel
    {
        private bool _isConnected;
        private string _curPlaceID;
        public CommentsViewModel()
        {
            SubmitVisitCommand = new Command(OnSubmitVisit);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SubmitVisitCommand.ChangeCanExecute();
            //try
            //{            
            //SubmitVisitCommand = new Command(OnSubmitVisit);
            //CancelCommand = new Command(OnCancel);
            //    this.PropertyChanged +=
            //        (_, __) => SubmitVisitCommand.ChangeCanExecute();
            //    //BindPlaceType();

            //    CancelCommand = new Command(OnCancel);
            //this.PropertyChanged +=
            //    (_, __) => CancelCommand.ChangeCanExecute();
            //IsConnected = CheckConnectivity();
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
        }
        public Command CancelCommand;
        public Command SubmitVisitCommand;

        private Stream imgStream;
        public Stream ImgStream
        {
            get => imgStream;
            set => SetProperty(ref imgStream, value);
        }
        private string id;
        public string ID
        {
            get => id;
            set => SetProperty(ref id, value);
        }
        private string title;
        public string CommentTitle
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string description;
        public string CommentDescription
        {
            get => description;
            set => SetProperty(ref description, value);
        }


        private DateTime createdAt;
        public DateTime CreatedAt
        {
            get => createdAt;
            set => SetProperty(ref createdAt, value);
        }

        private int rating;
        public int Rating
        {
            get => rating;
            set => SetProperty(ref rating, value);
        }


        private string ownerUserName;
        public string OwnerUserName {
            get => ownerUserName;
            set => SetProperty(ref ownerUserName, value);
        }

        public string CurPlaceID
        {
            get
            {
                return _curPlaceID;
            }
            set
            {
                _curPlaceID = value;
            }
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

        public bool IsConnected { 
        get => _isConnected;
            set { 
            _isConnected= value;
                OnPropertyChanged("IsConnected");
            }
        }
       
        private bool CheckConnectivity()
        {
            return Plugin.Connectivity.CrossConnectivity.Current.IsConnected;
        }
        private void OnCancel()
        { 
        }

        private async void OnSubmitVisit()
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(CommentTitle)
                    || string.IsNullOrEmpty(CommentDescription)
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
                        Visits objVisit = new Visits
                        {
                            id = Guid.NewGuid().ToString(),
                            title = CommentTitle,
                            placeId = _curPlaceID,
                            description = CommentDescription,
                            createdAt = System.DateTime.Now,
                            rating = Rating,
                            ImageName = ImageName,
                            ImageBytes = Image,
                            OwnerUserName = OwnerUserName
                        };
                        var objGetPlaceDetails = await MyPlaceService.SubmitComment(objVisit);
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
    }
}
