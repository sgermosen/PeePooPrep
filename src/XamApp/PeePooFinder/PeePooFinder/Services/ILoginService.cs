using PeePooFinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PeePooFinder.Services
{
    public interface ILoginService<T>
    {
        Task<LoginResponseModel> PerformLogin(T userModel);
    }
}
