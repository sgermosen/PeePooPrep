using Newtonsoft.Json;
using PeePooFinder.DataSettings;
using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Toast = PeePooFinder.Services.Toast;
namespace PeePooFinder.Services
{
    public class LoginService : ILoginService<LoginModel>
    {

        public LoginService()
        { 
        }

        public async Task<LoginResponseModel> PerformLogin(LoginModel userModel)
        {
            LoginResponseModel objLogIndetail = null;
            try
            {
                if (!string.IsNullOrEmpty(userModel.Email) || !string.IsNullOrEmpty(userModel.Password))
                {
                    string baseURL = APIData.Get_API_BaseURL() + "/api/account/login";
                    var json = JsonConvert.SerializeObject(userModel);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpClient _client = new HttpClient();
                    var task = await _client.PostAsync(baseURL, content);
                    if (task.IsSuccessStatusCode)
                    {
                        objLogIndetail = new LoginResponseModel();
                        var responsecontent = await task.Content.ReadAsStringAsync();
                        objLogIndetail = JsonConvert.DeserializeObject<LoginResponseModel>(responsecontent);

                    }
                    return await Task.FromResult(objLogIndetail);
                }
                return await Task.FromResult(objLogIndetail);

            }
            catch (Exception ex)
            {
                DependencyService.Get<Toast>().Show(ex.Message);
                return objLogIndetail;
            }
        }
    }
}
