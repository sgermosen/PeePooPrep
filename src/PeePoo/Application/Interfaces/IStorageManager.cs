using Application.Files;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStorageManager
    {
        Task<FileStorageResponse> Upload(string filename, string fullpath);
        Task<FileStorageResponse> GetFileAsync(string fileNameName);
        Task DeleteFile(string route);
        Task<string> EditFile(IFormFile file, string route);
        Task<string> SaveFile(IFormFile file);
        Task<IList<FileStorageResponse>> UploadBlobAsync(IList<IFormFile> filesName);
        Task<FileStorageResponse> UploadBlobAsync(IFormFile fileName);
        Task<FileStorageResponse> UploadBlobAsync(byte[] fileName);
        Task<FileStorageResponse> UploadBlobAsync(string imageName);
        Task DeleteBlobAsync(Guid idName);
        Task<FileStorageResponse> UploadTo(string filename, string basePath);

    }
}
