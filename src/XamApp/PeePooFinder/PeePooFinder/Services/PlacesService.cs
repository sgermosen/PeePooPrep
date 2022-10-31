using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Newtonsoft.Json;
using PeePooFinder.DataSettings;
using PeePooFinder.Helper;
using PeePooFinder.Models;
using PeePooFinder.Views;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Task = System.Threading.Tasks.Task;

namespace PeePooFinder.Services
{
    public class PlacesService : IPlacesService<Places>
    {
        private readonly FileUploadClient _fileUploadClient;
        private bool _keepUploading = true;
        public PlacesService()
        {
            _fileUploadClient = new FileUploadClient();
        }

        public async Task<List<Places>> GetAllPlacesAsync(bool forceRefresh = false)
        {
            try
            {
                List<Places> lstGetPlaceDetails = null;
                try
                {
                    string baseURL = APIData.Get_API_BaseURL() + "/api/places";
                    HttpClient _client = new HttpClient();
                    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                    var task = await _client.GetAsync(baseURL);
                    if (task.IsSuccessStatusCode)
                    {
                        lstGetPlaceDetails = new List<Places>();
                        var responsecontent = await task.Content.ReadAsStringAsync();
                        lstGetPlaceDetails = JsonConvert.DeserializeObject<List<Places>>(responsecontent);
                    }
                    return await System.Threading.Tasks.Task.FromResult(lstGetPlaceDetails);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Places> GetSinglePlaceAsync(string placeID)
        {
            Places objPlaceDetails = null;
            try
            {
                string baseURL = APIData.Get_API_BaseURL() + "/api/places/" + placeID + "";
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                var task = await _client.GetAsync(baseURL);
                if (task.IsSuccessStatusCode)
                {
                    objPlaceDetails = new Places();
                    var responsecontent = await task.Content.ReadAsStringAsync();
                    objPlaceDetails = JsonConvert.DeserializeObject<Places>(responsecontent);
                }
                return await System.Threading.Tasks.Task.FromResult(objPlaceDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Visit>> GetVisitsByPlaceID(string placeID)
        {
            List<Visit> lstPlaceDetails = null;
            try
            {
                //  "Visits/visitsFromPlace/934fa33b-ef3b-47b8-375d-08da377876c1"
                //placeID = "934fa33b-ef3b-47b8-375d-08da377876c1";
                ///string baseURL = APIData.Get_API_BaseURL() + "/api/places/" + placeID + "";
                string baseURL = APIData.Get_API_BaseURL() + "/api/Visits/visitsFromPlace/" + placeID + "";
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                var task = await _client.GetAsync(baseURL);
                if (task.IsSuccessStatusCode)
                {
                    lstPlaceDetails = new List<Visit>();
                    var responsecontent = await task.Content.ReadAsStringAsync();
                    lstPlaceDetails = JsonConvert.DeserializeObject<List<Visit>>(responsecontent);
                }
                return await System.Threading.Tasks.Task.FromResult(lstPlaceDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<double>> GetPlaceLocation(string placeID)
        {
            Places objPlaceDetails = null;
            List<double> lstLatLong = new List<double>();
            try
            {
                string baseURL = APIData.Get_API_BaseURL() + "/api/places/" + placeID + "";
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                var task = await _client.GetAsync(baseURL);
                if (task.IsSuccessStatusCode)
                {
                    objPlaceDetails = new Places();
                    var responsecontent = await task.Content.ReadAsStringAsync();
                    objPlaceDetails = JsonConvert.DeserializeObject<Places>(responsecontent);
                }
                if (objPlaceDetails != null)
                {
                    lstLatLong.Add(objPlaceDetails.lat);
                    lstLatLong.Add(objPlaceDetails.@long);
                }
                return await System.Threading.Tasks.Task.FromResult(lstLatLong);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Places>> GetAllPlacesAsync()
        {
            try
            {
                List<Places> lstGetPlaceDetails = null;
                try
                {
                    string baseURL = APIData.Get_API_BaseURL() + "/api/places";
                    HttpClient _client = new HttpClient();
                    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                    var task = await _client.GetAsync(baseURL);
                    if (task.IsSuccessStatusCode)
                    {
                        lstGetPlaceDetails = new List<Places>();
                        var responsecontent = await task.Content.ReadAsStringAsync();
                        lstGetPlaceDetails = JsonConvert.DeserializeObject<List<Places>>(responsecontent);
                    }
                    return await Task.FromResult(lstGetPlaceDetails);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SubmitResponseModel> SubmitPlace(SubmitModel _objsubmit)
        {
            SubmitResponseModel objSubmitResponse = new SubmitResponseModel();
            try
            {
                string baseURL = APIData.Get_API_BaseURL() + "/api/places";
                var fileBytes = _objsubmit.ImageBytes;
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                MultipartFormDataContent content = new MultipartFormDataContent();
                //if (fileBytes.Length > 5000000)
                //{
                //    resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 200, 200);
                //}
                //else
                //{
                //    resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 300, 300);
                //}

                //ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                //content.Add(byteContent, "File", _objsubmit.ImageName);

                if (fileBytes != null)
                {
                    // byte[] resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 400, 400);

                    byte[] resizedImage;
                    if (fileBytes.Length > 5000000)
                    {
                        resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 200, 200);
                    }
                    else
                    {
                        resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 300, 300);
                    }


                    //ByteArrayContent byteContent = new ByteArrayContent(resizedImage);
                    ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                    
                    content.Add(byteContent, "File", _objsubmit.ImageName);
                }

                string _placeID = Guid.NewGuid().ToString();
                StringContent id = new StringContent(_placeID);
                content.Add(id, "id");

                StringContent name = new StringContent(_objsubmit.name);
                content.Add(name, "Name");

                StringContent description = new StringContent(_objsubmit.description);
                content.Add(description, "description");

                StringContent type = new StringContent(_objsubmit.type);
                content.Add(type, "type");

                StringContent createdAt = new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"));
                content.Add(createdAt, "createdAt");

                StringContent rating = new StringContent(_objsubmit.rating.ToString());
                content.Add(rating, "rating");

                StringContent @long = new StringContent(_objsubmit.@long.ToString());
                content.Add(@long, "long");

                StringContent lat = new StringContent(_objsubmit.lat.ToString());
                content.Add(lat, "lat");

                StringContent observations = new StringContent(_objsubmit.observations);
                content.Add(observations, "observations");

                StringContent HaveBabyChanger = new StringContent(_objsubmit.haveBabyChanger.ToString());
                content.Add(HaveBabyChanger, "HaveBabyChanger");

                StringContent IsRoomy = new StringContent(_objsubmit.isRoomy.ToString());
                content.Add(IsRoomy, "IsRoomy");

                StringContent IsAvailable = new StringContent(_objsubmit.isAvailable.ToString());
                content.Add(IsAvailable, "IsAvailable");

                //StringContent IsAproved = new StringContent(_objsubmit.IsAproved.ToString());
                //content.Add(IsAproved, "IsAproved");
                

                StringContent Urinals = new StringContent(_objsubmit.urinals.ToString());
                content.Add(Urinals, "Urinals");

                StringContent Toilets = new StringContent(_objsubmit.toilets.ToString());
                content.Add(Toilets, "Toilets");
                StringContent userName = new StringContent(_objsubmit.OwnerUserName.ToString());
                content.Add(userName, "ownerUserName");
                string FileName = Guid.NewGuid().ToString() + ".jpg";
                StringContent imageName = new StringContent(FileName);//new StringContent(_objsubmit.ImageName.ToString());
                content.Add(imageName, "Image");



                string retVal = "";
                var response = await _client.PostAsync(baseURL, content);
                if (response.IsSuccessStatusCode)
                {
                    //string orgFileName = FileName;// imageName.ToString();// _objsubmit.ImageName;
                    //var fileHandle = await _fileUploadClient.BeginFileUpload(orgFileName);
                    //const int maxChunkSize = (512 * 1024); //Send attachments in chunks of 512 KB at a time.
                    //var buffer = new byte[maxChunkSize];
                    //long fileSize = 0;
                    //long totalBytesRead = 0;
                    //using (var fs = new MemoryStream(_objsubmit.ImageBytes))
                    //{
                    //    var bytesRead = 0;
                    //    fileSize = fs.Length;
                    //    do
                    //    {
                    //        var position = fs.Position;
                    //        bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                    //        if (bytesRead < buffer.Length)
                    //        {
                    //            //If the bytes read is smaller then the buffer the last chunk will be sent.
                    //            //Shrink the buffer to fit the last bytes to optimize memory usage.
                    //            Array.Resize(ref buffer, bytesRead);
                    //        }

                    //        await _fileUploadClient.UploadChunk(fileHandle, buffer, position);
                    //        totalBytesRead += bytesRead;
                    //        CalculateProgress(totalBytesRead, fileSize); //Update the progress bar.
                    //    } while (bytesRead > 0 && _keepUploading);
                    //}
                    //if (!_keepUploading)
                    //{
                    //    //Cancel the upload
                    //    await _fileUploadClient.EndFileUpload(fileHandle, fileSize, orgFileName, true);
                    //}
                    //else
                    //{
                    //    var uploadComplete = await _fileUploadClient.EndFileUpload(fileHandle, fileSize, orgFileName);
                    //}
                    var responsecontent = await response.Content.ReadAsStringAsync();
                    //retVal = JsonConvert.DeserializeObject<string>(responsecontent);
                    objSubmitResponse = new SubmitResponseModel();
                    objSubmitResponse.Message = "Place Submitted Successfully";
                    objSubmitResponse.Status = "success";
                }
                else
                {
                    objSubmitResponse = new SubmitResponseModel();
                    //objSubmitResponse.Message = "There is some issue in submitting the place. Please try again later. If you are facing this issue continuously then please contact administrator";
                    objSubmitResponse.Message = await response.Content.ReadAsStringAsync();
                    objSubmitResponse.Message = objSubmitResponse.Message + "\nStatus Code:" + response.StatusCode.ToString();
                    objSubmitResponse.Status = "fail";
                }
                return objSubmitResponse; //StatusLabel.Text = response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private void CalculateProgress(long completed, long total)
        {
            var comp = Convert.ToDouble(completed);
            var tot = Convert.ToDouble(total);
            var percentage = comp / tot;
            //UploadProgress.ProgressTo(percentage, 100, Easing.Linear);
        }

        public async Task<SubmitResponseModel> SubmitComment(Visits _objsubmit)
        {
            SubmitResponseModel objSubmitResponse = new SubmitResponseModel();
            try
            {
                string baseURL = APIData.Get_API_BaseURL() + "/api/visits";
                var fileBytes = _objsubmit.ImageBytes;
                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
                MultipartFormDataContent content = new MultipartFormDataContent();
                if (fileBytes != null)
                {
                   // byte[] resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 400, 400);

                    byte[] resizedImage;
                    if (fileBytes.Length > 5000000)
                    {
                        resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 200, 200);
                    }
                    else
                    {
                        resizedImage = await ImageResizeHelper.ResizeImage(fileBytes, 300, 300);
                    }


                    ByteArrayContent byteContent = new ByteArrayContent(resizedImage);
                    content.Add(byteContent, "File", _objsubmit.ImageName);
                }
                StringContent id = new StringContent(Guid.NewGuid().ToString());
                content.Add(id, "id");

                StringContent placeId = new StringContent(_objsubmit.placeId);
                content.Add(placeId, "placeId");

                StringContent title = new StringContent(_objsubmit.title);
                content.Add(title, "title");

                StringContent description = new StringContent(_objsubmit.description);
                content.Add(description, "description");

                //StringContent createdAt = new StringContent(System.DateTime.Now.ToString());
                StringContent createdAt = new StringContent(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"));
                content.Add(createdAt, "createdAt");

                StringContent rating = new StringContent(_objsubmit.rating.ToString());
                content.Add(rating, "rating");

                StringContent userName = new StringContent(_objsubmit.OwnerUserName.ToString());
                content.Add(userName, "ownerUserName");
                StringContent ownerUserName = new StringContent(_objsubmit.OwnerUserName.ToString());
                content.Add(ownerUserName, "ownerUserName");

                string retVal = "";
                var response = await _client.PostAsync(baseURL, content);
                if (response.IsSuccessStatusCode)
                {
                    var responsecontent = await response.Content.ReadAsStringAsync();
                    //retVal = JsonConvert.DeserializeObject<string>(responsecontent);
                    objSubmitResponse = new SubmitResponseModel();
                    objSubmitResponse.Message = "Comment Added successfully";
                    objSubmitResponse.Status = "success";
                }
                else
                {
                    objSubmitResponse = new SubmitResponseModel();
                    objSubmitResponse.Message = "There is some issue in submitting the Review. Please try again later. If you are facing this issue continuously then please contact administrator";
                    objSubmitResponse.Status = "fail";
                }
                return objSubmitResponse; //StatusLabel.Text = response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
