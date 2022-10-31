using Application.Photos;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPhotoAccessor
    {
        Task<PhotoUploadResult> AddPhoto(IFormFile file);
        Task<PhotoUploadResult> AddPhotoLarge(Stream streamdata, string FileName);
        Task<PhotoUploadResult> AddPhotoLargeFile(IFormFile file);
        Task<string> DeletePhoto(string publicId);
    }
}