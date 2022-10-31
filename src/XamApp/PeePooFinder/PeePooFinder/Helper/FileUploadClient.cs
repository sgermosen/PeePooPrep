using PeePooFinder.DataSettings;
using PeePooFinder.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PeePooFinder.Helper
{
 /// <summary>
        /// Class that talks to the Media controller of the API that takes care of the incident media uploading and streaming.
        /// </summary>
        public class FileUploadClient
        {
            // private const string _baseUrl = "http://localhost:2182";
            //private const string _baseUrl = "http://lillgreenz.in";
            private const string _baseUrl = "https://peepoo.azurewebsites.net";
        private readonly HttpClient _client;
            private readonly IApi _apiRestInstance;

        public FileUploadClient()
        {
            _client = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(60)
            };
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToString(Application.Current.Properties["Token"]));
            _apiRestInstance = RestService.For<IApi>(_client);
        }

            /// <summary>
            /// Start uploading a new file to the server.
            /// This method will allocate a unique file handle and create an empty file in a temporary upload folder.
            /// </summary>
            /// <param name="fileName">The name of the file to upload. This name will be used in the created file handle.</param>
            /// <returns>The created file handle when the file was successfully allocated. Or null if a file with that name is already being uploaded or an error occurred.</returns>
            public async Task<string> BeginFileUpload(string fileName)
            {
                var response = await _apiRestInstance.BeginFileUpload(fileName);
                if (string.IsNullOrEmpty(response))
                    return null; //The fileHandle could not be retrieved.

                response = response.Replace("\"", ""); //Remove the quotes inside the string.
                return response;
            }

            /// <summary>
            /// Upload a part of a media file.
            /// This method takes a part of the media file and appends it to the incomplete file. This method is to be called repeatedly until the upload is complete.
            /// </summary>
            /// <param name="fileHandle">The fileHandle of the file that is being uploaded.</param>
            /// <param name="data">The data of the file that is part of this chunk.</param>
            /// <param name="startAt">The position to start appending this chunk of data onto the incomplete file upload.</param>
            public async Task UploadChunk(string fileHandle, byte[] data, long startAt)
            {
                var base64Data = Convert.ToBase64String(data);
                var chunk = new MediaChunkDto()
                {
                    FileHandle = fileHandle,
                    Data = base64Data,
                    StartAt = startAt.ToString()
                };

                await _apiRestInstance.UploadChunk(chunk);
            }

            /// <summary>
            /// Finish a file upload and copy the completed file to the upload folder so it can be streamed or retrieved.
            /// </summary>
            /// <param name="fileHandle">The file handle of the file that the upload is complete for.</param>
            /// <param name="quitUpload">If this is true the file upload will be aborted. The temporary file will be deleted.</param>
            /// <param name="fileSize">The size of the original file that was uploaded. This is used to check if the upload was successful.</param>
            /// <returns>True if the upload was successfully ended. False if the file handle was not found. Or if the file could not be moved or deleted.</returns>
            public async Task<bool> EndFileUpload(string fileHandle, long fileSize, string OrgFileName, bool quitUpload = false)
            {
                try
                {
                    await _apiRestInstance.EndFileUpload(fileHandle, quitUpload, fileSize, OrgFileName);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public async Task<bool> UploadFile(MultipartFormDataContent content)
            {
                try
                {
                    await _apiRestInstance.UploadFile(content);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
}
