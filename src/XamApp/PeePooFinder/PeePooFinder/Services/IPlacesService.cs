using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PeePooFinder.Services
{
    public interface IPlacesService<T>
    {
        Task<List<T>> GetAllPlacesAsync(bool forceRefresh = false);
        //Task<List<Places>> GetAllPlacesAsync(LoginResponseModel userModel);

        Task<Places> GetSinglePlaceAsync(string placeID);
        Task<List<Visit>> GetVisitsByPlaceID(string placeID);
        
        Task<List<Places>> GetAllPlacesAsync();
        Task<SubmitResponseModel> SubmitPlace(SubmitModel _objsubmit);
        Task<SubmitResponseModel> SubmitComment(Visits _objsubmit);
        
        Task<List<double>> GetPlaceLocation(string PlaceID);
        //Task<SubmitResponseModel> SubmitPlace(SubmitModel objGetPlaceDetails);
        //Task<SubmitResponseModel> SubmitPlace(Places _objsubmit);
    }
}
