using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Application.Files
{
    public class AzureStorageManager : IStorageManager
    {
        private readonly string _connectionString;
        private readonly ILogger<AzureStorageManager> _logger;
        private readonly string _container;

        public AzureStorageManager(IConfiguration configuration, ILogger<AzureStorageManager> logger)
        {
            _logger = logger;
            _container = configuration["Blob:ContainerName"];
            _connectionString = configuration["Blob:ConnectionString"];
        }

        public async Task<FileStorageResponse> Upload(string filename, string fullpath)
        {

            _logger.LogInformation("In Service Upload Start");

            var fileBytes = File.ReadAllBytes(fullpath);
            MemoryStream stream = new MemoryStream(fileBytes);

            string extension, fileName, name;
            SetNameAndExtention(filename, out name, out extension, out fileName);
            BlobContainerClient client = await SetClient();
            var blobClient = client.GetBlobClient(fileName);
            BlobHttpHeaders httpHeaders = SetHeader(filename);
            await blobClient.UploadAsync(stream, httpHeaders);

            _logger.LogInformation($"In Service Upload Done: {client.Uri.AbsoluteUri}/{fileName}");
            return ReturnValue(fileBytes, name, extension, fileName, client);

        }

        private static FileStorageResponse ReturnValue(byte[] fileBytes, string name, string extension, string fileName, BlobContainerClient client)
        {
            return new FileStorageResponse
            {
                Name = name,
                Extension = extension,
                Url = $"{client.Uri.AbsoluteUri}/{fileName}",
                Size = fileBytes.Length,
                FileName = fileName// fileobject.Name
            };
        }

        private static BlobHttpHeaders SetHeader(string filename)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string file_type))
                file_type = "application/octet-stream";
            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file_type
            };
            return httpHeaders;
        }

        private async Task<BlobContainerClient> SetClient()
        {
            var client = new BlobContainerClient(_connectionString, _container);
            await client.CreateIfNotExistsAsync();
            return client;
        }

        private static void SetNameAndExtention(string filename, out string name, out string extension, out string fileName)
        {
            name = Guid.NewGuid().ToString().Replace("-", "");
            extension = Path.GetExtension(filename);
            fileName = $"{name}{extension}";
        }

        //-------
        public async Task<FileStorageResponse> GetFileAsync(string fileName)
        {

            var client = new BlobContainerClient(_connectionString, _container);
            var blobClient = client.GetBlobClient(fileName);
            var downloadContent = await blobClient.DownloadAsync();
            var extension = Path.GetExtension(fileName);

            using MemoryStream ms = new MemoryStream();
            await downloadContent.Value.Content.CopyToAsync(ms);
            return new FileStorageResponse
            {
                FileName = fileName,
                Extension = extension,
                Url = $"{client.Uri.AbsoluteUri}/{fileName}",
                File = ms.ToArray()
            };
        }

        public async Task<FileStorageResponse> UploadBlobAsync(byte[] file)
        {
            MemoryStream stream = new MemoryStream(file);
            var name = Guid.NewGuid().ToString().Replace("-", "");

            var extension = Path.GetExtension(file.ToString());
            var fileName = $"{name}{extension}";
            BlobContainerClient client = await SetClient();

            var blobClient = client.GetBlobClient(fileName);
            BlobHttpHeaders httpHeaders = SetHeader(fileName);
            await blobClient.UploadAsync(stream, httpHeaders);

            return ReturnValue(file, name, extension, fileName, client);

        }

        public async Task<FileStorageResponse> UploadBlobAsync(IFormFile file)
        {
            Stream stream = file.OpenReadStream();
            var name = Guid.NewGuid().ToString().Replace("-", "");

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{name}{extension}";
            BlobContainerClient client = await SetClient();
            var blobClient = client.GetBlobClient(fileName);
            BlobHttpHeaders httpHeaders = SetHeader(fileName);
            await blobClient.UploadAsync(stream, httpHeaders);
            return new FileStorageResponse
            {
                Extension = extension,
                Size = file.Length,
                FileName = fileName,
                Name = name,
                Url = $"{client.Uri.AbsoluteUri}/{fileName}"
            };

        }

        public async Task<IList<FileStorageResponse>> UploadBlobAsync(IList<IFormFile> files)
        {
            BlobContainerClient client = await SetClient();
            var responses = new List<FileStorageResponse>();
            foreach (var file in files)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    var name = Guid.NewGuid().ToString().Replace("-", "");
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{name}{extension}";

                    //await client.UploadBlobAsync(fileName, stream);
                    var blobClient = client.GetBlobClient(fileName);
                    BlobHttpHeaders httpHeaders = SetHeader(fileName);
                    await blobClient.UploadAsync(stream, httpHeaders);

                    responses.Add(new FileStorageResponse
                    {
                        Extension = extension,
                        FileName = fileName,
                        Size = file.Length,
                        Name = name,
                        Url = $"{client.Uri.AbsoluteUri}/{fileName}"
                    });
                }
            }
            return responses;
        }

        public async Task<FileStorageResponse> UploadBlobAsync(string image)
        {
            Stream stream = File.OpenRead(image);
            var name = Guid.NewGuid().ToString().Replace("-", "");
            var extension = Path.GetExtension(image);
            var fileName = $"{name}{extension}";
            BlobContainerClient client = await SetClient();
            var blobClient = client.GetBlobClient(fileName);
            BlobHttpHeaders httpHeaders = SetHeader(fileName);
            await blobClient.UploadAsync(stream, httpHeaders);
            return new FileStorageResponse
            {
                Extension = extension,
                Size = stream.Length,
                FileName = fileName,
                Name = name,
                Url = $"{client.Uri.AbsoluteUri}/{fileName}"
            };
        }

        public async Task DeleteBlobAsync(Guid id)
        {
            var client = new BlobContainerClient(_connectionString, _container);
            var blobClient = client.GetBlobClient(id.ToString());
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var client = new BlobContainerClient(_connectionString, _container);
            await client.CreateIfNotExistsAsync();

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}{extension}";
            var blob = client.GetBlobClient(fileName);
            await blob.UploadAsync(file.OpenReadStream());
            return blob.Uri.ToString();

        }

        public async Task DeleteFile(string route)
        {
            if (string.IsNullOrEmpty(route))
                return;
            var client = new BlobContainerClient(_connectionString, _container);
            await client.CreateIfNotExistsAsync();
            var file = Path.GetFileName(route);
            var blob = client.GetBlobClient(file);
            await blob.DeleteIfExistsAsync();

        }

        public async Task<string> EditFile(IFormFile file, string route)
        {
            await DeleteFile(route);
            return await SaveFile(file);
        }
    }
}
