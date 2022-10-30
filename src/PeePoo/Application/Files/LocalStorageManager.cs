using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Application.Files
{
    public class LocalStorageManager : IStorageManager
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;
        string container = "";
        public LocalStorageManager(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(env.WebRootPath, container);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string route = Path.Combine(folder, fileName);

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                await File.WriteAllBytesAsync(route, content);
            }
            var actualUrl = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
            var routeForDb = Path.Combine(actualUrl, container, fileName).Replace("\\", "/");
            return routeForDb;
        }

        public Task<FileStorageResponse> UploadBlobAsync(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public Task<FileStorageResponse> UploadBlobAsync(byte[] file)
        {
            throw new NotImplementedException();
        }

        public Task<FileStorageResponse> UploadBlobAsync(string image)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBlobAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFile(string route)
        {
            if (string.IsNullOrEmpty(route))
                return Task.CompletedTask;

            var filename = Path.Combine(route);
            var fileDirectory = Path.Combine(env.WebRootPath, container, filename);

            if (File.Exists(fileDirectory))
                File.Delete(fileDirectory);

            return Task.CompletedTask;
        }

        public async Task<string> EditFile(IFormFile file, string route)
        {
            await DeleteFile(route);
            return await SaveFile(file);
        }

        public Task<byte[]> GetFileAsync(string fileName, string containerName)
        {
            throw new NotImplementedException();
        }

        Task<FileStorageResponse> IStorageManager.GetFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<IList<FileStorageResponse>> UploadBlobAsync(IList<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        public Task<FileStorageResponse> Upload(string filename, string basePath)
        {
            throw new NotImplementedException();
        }

        public Task<FileStorageResponse> UploadTo(string filename, string basePath)
        {
            throw new NotImplementedException();
        }
    }
}
