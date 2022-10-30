using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Application.Files
{
    public class AzureStorageManager : IStorageManager
    {
        private readonly string _connectionString;
        // Set this variable to true if you want to authenticate Interactively through the browser using your Azure user account
        private const bool UseInteractiveAuth = false;
        private const string AdaptiveStreamingTransformName = "MyTransformWithAdaptiveStreamingPreset";
        private const string OutputFolderName = @"Output";
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
            client.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

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

        //private readonly IAzureMediaServiceUtility _azureMediaServiceUtility;
        //private readonly ILogger<StreamingVideoService> _logger;
        //private readonly AppSettings _appSettings;
        //private readonly IMessagePublisher _messagePublisher;
        //private readonly AzureServiceBusSettings _azureServiceBusSettings;
        //private IAzureMediaServicesClient _azureMediaServicesClient;
        //private string _jobName;
        //private string _inputAssetName;
        //public StreamingVideoService(
        //    IAzureMediaServiceUtility azureMediaServiceUtility,
        //    ILogger<StreamingVideoService> logger,
        //    IOptions<AppSettings> appSettings,
        //    IMessagePublisher messagePublisher,
        //    IOptions<AzureServiceBusSettings> azureServiceBusSettings)
        //{
        //    _azureMediaServiceUtility = azureMediaServiceUtility;
        //    _logger = logger;
        //    _appSettings = appSettings.Value;
        //    _messagePublisher = messagePublisher;
        //    _azureServiceBusSettings = azureServiceBusSettings.Value;
        //}
        //public async Task<EncodedVideoResult> UploadAndEncodeVideoForStreaming(string videoFileName, string receiptImageSasUri, string storeNumber, string incidentId, string cameraId, string locationId)
        //{
        //    try
        //    {
        //        var videoContainerNormalizedName = videoFileName.ToLower().Replace("_", "-").Replace(".mp4", string.Empty);
        //        _azureMediaServicesClient = await _azureMediaServiceUtility.CreateMediaServicesClientAsync();
        //        _azureMediaServicesClient.LongRunningOperationRetryTimeout = 2;

        //        string uniqueness = $"s{storeNumber}-{DateTime.Now.ToString("MMMMyyyy").ToLower()}-{Guid.NewGuid().ToString("N")}";
        //        _jobName = $"job-{uniqueness}";
        //        string locatorName = $"locator-{uniqueness}";
        //        string outputAssetName = $"output-{uniqueness}";
        //        _inputAssetName = $"input-{uniqueness}";

        //        _ = await _azureMediaServiceUtility.GetOrCreateTransformAsync(_azureMediaServicesClient, "PosExceptionStreamingTransform");
        //        _ = await _azureMediaServiceUtility.CreateInputAssetAsync(_azureMediaServicesClient, _inputAssetName, Path.Combine(_appSettings.VideoPath, videoFileName));
        //        _ = new JobInputAsset(assetName: _inputAssetName);

        //        var outputAsset = await _azureMediaServiceUtility.CreateOutputAssetAsync(_azureMediaServicesClient, outputAssetName);

        //        _ = await _azureMediaServiceUtility.SubmitJobAsync(_azureMediaServicesClient, "PosExceptionStreamingTransform", _jobName, _inputAssetName, outputAsset.Name);

        //        var job = await _azureMediaServiceUtility.WaitForJobToFinishAsync(_azureMediaServicesClient, "PosExceptionStreamingTransform", _jobName);
        //        if (job.State == JobState.Finished)
        //        {
        //            var locator = await _azureMediaServiceUtility.CreateStreamingLocatorAsync(_azureMediaServicesClient, outputAssetName, locatorName);

        //            var urls = await _azureMediaServiceUtility.GetStreamingUrlsAsync(_azureMediaServicesClient, locator.Name);
        //            var videoDownloadUrl = urls.Where(x => x.Contains(".mp4")).FirstOrDefault();
        //            await _azureMediaServiceUtility.DownloadOutputAssetAsync(_azureMediaServicesClient, outputAssetName, _appSettings.VideoPath);
        //            var videoInfo = ExtractVideoInformationFromManifest(videoFileName);

        //            DeleteFromLocal(videoFileName);
        //            return new EncodedVideoResult
        //            {
        //                IncidentId = incidentId,
        //                VideoDownloadUrl = videoDownloadUrl,
        //                VideoWeight = videoInfo.VideoWeight,
        //                VideoDuration = videoInfo.VideoDuration,
        //                ReceiptImageSasUri = receiptImageSasUri,
        //                CameraId = cameraId,
        //                LocationId = locationId
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _messagePublisher.Publish(new HighlightAsUnableToDownloadVideo(incidentId), _azureServiceBusSettings.VideoServiceTopic);
        //        _logger.LogError(ex, ex.Message);
        //    }
        //    finally
        //    {
        //        await _azureMediaServiceUtility.CleanUpAsync(_azureMediaServicesClient, "PosExceptionStreamingTransform", _jobName, new List<string>() { _inputAssetName });
        //    }
        //    return new EncodedVideoResult();
        //}
        //public void DeleteFromLocal(string videoFileName)
        //{
        //    try
        //    {
        //        var localVideoFilePath = Path.Combine(_appSettings.VideoPath, videoFileName);
        //        var localManifestFilePath = Path.Combine(_appSettings.VideoPath, videoFileName.Replace(".mp4", "_manifest.json"));
        //        var localReceiptImageFilePath = Path.Combine(_appSettings.ReceiptImagePath, videoFileName.Replace(".mp4", ".png"));

        //        File.Delete(localVideoFilePath);
        //        File.Delete(localManifestFilePath);
        //        File.Delete(localReceiptImageFilePath);

        //        _logger.LogInformation($"{videoFileName} was deleted from local folder succeed");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //    }
        //}

        /// <summary>
        /// Run the sample async.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        public async Task<FileStorageResponse> UploadTo(string filename, string basePath)
        {
            var result = new FileStorageResponse { FileName = filename };
            // If Visual Studio is used, let's read the .env file which should be in the root folder (same folder than the solution .sln file).
            // Same code will work in VS Code, but VS Code uses also launch.json to get the .env file.
            // You can create this ".env" file by saving the "sample.env" file as ".env" file and fill it with the right values.
            //try
            //{
            //    DotEnv.Load(".env");
            //}
            //catch
            //{

            //}

            ConfigWrapper config = new(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() // parses the values from the optional .env file at the solution root
                .Build());

            IAzureMediaServicesClient client;
            try
            {
                client = await Authentication.CreateMediaServicesClientAsync(config, UseInteractiveAuth);
            }
            catch (Exception e)
            {
                //Console.Error.WriteLine("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                //Console.Error.WriteLine($"{e.Message}");
                return null;
            }

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            // Creating a unique suffix so that we don't have name collisions if you run the sample
            // multiple times without cleaning up.
            string uniqueness = Guid.NewGuid().ToString("N");
            string jobName = $"job-{uniqueness}";
            string locatorName = $"locator-{uniqueness}";
            string outputAssetName = $"output-{uniqueness}";
            string inputAssetName = $"input-{uniqueness}";

            // Ensure that you have the desired encoding Transform. This is really a one time setup operation.
            _ = await GetOrCreateTransformAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName);

            // Create a new input Asset and upload the specified local video file into it.
            _ = await CreateInputAssetAsync(client, config.ResourceGroup, config.AccountName, inputAssetName, filename, basePath);

            // Use the name of the created input asset to create the job input.
            _ = new JobInputAsset(assetName: inputAssetName);

            // Output from the encoding Job must be written to an Asset, so let's create one
            Asset outputAsset = await CreateOutputAssetAsync(client, config.ResourceGroup, config.AccountName, outputAssetName);

            _ = await SubmitJobAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName, inputAssetName, outputAsset.Name);
            // In this demo code, we will poll for Job status
            // Polling is not a recommended best practice for production applications because of the latency it introduces.
            // Overuse of this API may trigger throttling. Developers should instead use Event Grid.
            Job job = await WaitForJobToFinishAsync(client, config.ResourceGroup, config.AccountName, AdaptiveStreamingTransformName, jobName);

            if (job.State == JobState.Finished)
            {
                //Console.WriteLine("Job finished.");
                if (!Directory.Exists(OutputFolderName))
                    Directory.CreateDirectory(OutputFolderName);

                await DownloadOutputAssetAsync(client, config.ResourceGroup, config.AccountName, outputAsset.Name, OutputFolderName);

                StreamingLocator locator = await CreateStreamingLocatorAsync(client, config.ResourceGroup, config.AccountName, outputAsset.Name, locatorName);

                // Note that the URLs returned by this method include a /manifest path followed by a (format=)
                // parameter that controls the type of manifest that is returned. 
                // The /manifest(format=m3u8-aapl) will provide Apple HLS v4 manifest using MPEG TS segments.
                // The /manifest(format=mpd-time-csf) will provide MPEG DASH manifest.
                // And using just /manifest alone will return Microsoft Smooth Streaming format.
                // There are additional formats available that are not returned in this call, please check the documentation
                // on the dynamic packager for additional formats - see https://docs.microsoft.com/azure/media-services/latest/dynamic-packaging-overview
                IList<string> urls = await GetStreamingUrlsAsync(client, config.ResourceGroup, config.AccountName, locator.Name);
                //foreach (var url in urls)
                //{
                //    Console.WriteLine(url);
                //}
                result.Url = urls.FirstOrDefault();
            }
            return result;
            //Console.WriteLine("Done. Copy and paste the Streaming URL ending in '/manifest' into the Azure Media Player at 'http://aka.ms/azuremediaplayer'.");
            //Console.WriteLine("See the documentation on Dynamic Packaging for additional format support, including CMAF.");
            //Console.WriteLine("https://docs.microsoft.com/azure/media-services/latest/dynamic-packaging-overview");

        }

        //public async Task<FileContentResult> WriteContentToStream()
        //{
        //    var cloudBlob = await _blobClient.GetBlobAsync(PlatformServiceConstants._blobIntroductoryVideoContainerPath + PlatformServiceConstants.IntroductoryVideo1, introductoryvideocontainerName);

        //    MemoryStream fileStream = new MemoryStream();
        //    await cloudBlob.DownloadToStreamAsync(fileStream);
        //    return new FileContentResult(fileStream.ToArray(), "application/octet-stream");

        //}


        /// <summary>
        /// Creates a new input Asset and uploads the specified local video file into it.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The asset name.</param>
        /// <param name="fileToUpload">The file you want to upload into the asset.</param>
        /// <returns></returns>
        // <CreateInputAsset>
        private static async Task<Asset> CreateInputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string assetName,
            string fileToUpload, string fullPath)
        {
            // In this example, we are assuming that the asset name is unique.
            //
            // If you already have an asset with the desired name, use the Assets.Get method
            // to get the existing asset. In Media Services v3, the Get method on entities returns null 
            // if the entity doesn't exist (a case-insensitive check on the name).

            // Call Media Services API to create an Asset.
            // This method creates a container in storage for the Asset.
            // The files (blobs) associated with the asset will be stored in this container.
            Asset asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions 
            // and the exparation time for the SAS URL.
            var response = await client.Assets.ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.  
            BlobContainerClient container = new BlobContainerClient(sasUri);
            BlobClient blob = container.GetBlobClient(fileToUpload);

            // Use Strorage API to upload the file into the container in storage.
            await blob.UploadAsync(fullPath);

            return asset;
        }
        // </CreateInputAsset>

        /// <summary>
        /// Creates an ouput asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <returns></returns>
        // <CreateOutputAsset>
        private static async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            bool existingAsset = true;
            Asset outputAsset;
            try
            {
                // Check if an Asset already exists
                outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                existingAsset = false;
            }

            Asset asset = new Asset();
            string outputAssetName = assetName;

            if (existingAsset)
            {
                // Name collision! In order to get the sample to work, let's just go ahead and create a unique asset name
                // Note that the returned Asset can have a different name than the one specified as an input parameter.
                // You may want to update this part to throw an Exception instead, and handle name collisions differently.
                string uniqueness = $"-{Guid.NewGuid():N}";
                outputAssetName += uniqueness;

                //Console.WriteLine("Warning – found an existing Asset with name = " + assetName);
                //Console.WriteLine("Creating an Asset with this name instead: " + outputAssetName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
        }
        // </CreateOutputAsset>

        /// <summary>
        /// If the specified transform exists, get that transform.
        /// If the it does not exist, creates a new transform with the specified output. 
        /// In this case, the output is set to encode a video using one of the built-in encoding presets.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <returns></returns>
        // <EnsureTransformExists>
        private static async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName)
        {
            bool createTransform = false;
            Transform transform = null;
            try
            {
                // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
                // also uses the same recipe or Preset for processing content.
                transform = client.Transforms.Get(resourceGroupName, accountName, transformName);
            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                createTransform = true;
            }

            if (createTransform)
            {
                // You need to specify what you want it to produce as an output
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput
                    {
                        // The preset for the Transform is set to one of Media Services built-in sample presets.
                        // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            // This sample uses the built-in encoding preset for Adaptive Bitrate Streaming.
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }

            return transform;
        }
        // </EnsureTransformExists>

        /// <summary>
        /// Submits a request to Media Services to apply the specified Transform to a given input video.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The (unique) name of the job.</param>
        /// <param name="inputAssetName">The name of the input asset.</param>
        /// <param name="outputAssetName">The (unique) name of the  output asset that will store the result of the encoding job. </param>
        // <SubmitJob>
        private static async Task<Job> SubmitJobAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            string inputAssetName,
            string outputAssetName)
        {
            // Use the name of the created input asset to create the job input.
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName);

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            // In this example, we are assuming that the job name is unique.
            //
            // If you already have a job with the desired name, use the Jobs.Get method
            // to get the existing job. In Media Services v3, the Get method on entities returns null 
            // if the entity doesn't exist (a case-insensitive check on the name).
            Job job = await client.Jobs.CreateAsync(
                resourceGroupName,
                accountName,
                transformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            return job;
        }
        // </SubmitJob>

        /// <summary>
        /// Polls Media Services for the status of the Job.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The name of the job you submitted.</param>
        /// <returns></returns>
        // <WaitForJobToFinish>
        private static async Task<Job> WaitForJobToFinishAsync(IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName)
        {
            const int SleepIntervalMs = 20 * 1000;

            Job job;
            do
            {
                job = await client.Jobs.GetAsync(resourceGroupName, accountName, transformName, jobName);

                //Console.WriteLine($"Job is '{job.State}'.");
                //for (int i = 0; i < job.Outputs.Count; i++)
                //{
                //    JobOutput output = job.Outputs[i];
                //    //Console.Write($"\tJobOutput[{i}] is '{output.State}'.");
                //    if (output.State == JobState.Processing)
                //    {
                //        Console.Write($"  Progress (%): '{output.Progress}'.");
                //    }

                //    Console.WriteLine();
                //}

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }
        // </WaitForJobToFinish>

        /// <summary>
        /// Creates a StreamingLocator for the specified asset and with the specified streaming policy name.
        /// Once the StreamingLocator is created the output asset is available to clients for playback.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The name of the output asset.</param>
        /// <param name="locatorName">The StreamingLocator name (unique in this case).</param>
        /// <returns></returns>
        // <CreateStreamingLocator>
        private static async Task<StreamingLocator> CreateStreamingLocatorAsync(
            IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName,
            string locatorName)
        {
            StreamingLocator locator = await client.StreamingLocators.CreateAsync(
                resourceGroup,
                accountName,
                locatorName,
                new StreamingLocator
                {
                    AssetName = assetName,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                });

            return locator;
        }
        // </CreateStreamingLocator>

        /// <summary>
        /// Checks if the "default" streaming endpoint is in the running state,
        /// if not, starts it.
        /// Then, builds the streaming URLs.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="locatorName">The name of the StreamingLocator that was created.</param>
        /// <returns></returns>
        // <GetStreamingURLs>
        private static async Task<IList<string>> GetStreamingUrlsAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            String locatorName)
        {
            const string DefaultStreamingEndpointName = "default";

            IList<string> streamingUrls = new List<string>();

            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);

            if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
            {
                await client.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
            }

            ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName,

                    Path = path.Paths[0]
                };
                streamingUrls.Add(uriBuilder.ToString());
            }
            return streamingUrls;
        }
        // </GetStreamingURLs>

        /// <summary>
        ///  Downloads the results from the specified output asset, so you can see what you got.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset.</param>
        /// <param name="outputFolderName">The name of the folder into which to download the results.</param>
        // <DownloadResults>
        private static async Task DownloadOutputAssetAsync(
            IAzureMediaServicesClient client,
            string resourceGroup,
            string accountName,
            string assetName,
            string outputFolderName)
        {
            if (!Directory.Exists(outputFolderName))
            {
                Directory.CreateDirectory(outputFolderName);
            }

            AssetContainerSas assetContainerSas = await client.Assets.ListContainerSasAsync(
                resourceGroup,
                accountName,
                assetName,
                permissions: AssetContainerPermission.Read,
                expiryTime: DateTime.UtcNow.AddHours(1).ToUniversalTime());

            Uri containerSasUrl = new Uri(assetContainerSas.AssetContainerSasUrls.FirstOrDefault());
            BlobContainerClient container = new BlobContainerClient(containerSasUrl);

            string directory = Path.Combine(outputFolderName, assetName);
            Directory.CreateDirectory(directory);

            //Console.WriteLine($"Downloading output results to '{directory}'...");

            string continuationToken = null;
            IList<Task> downloadTasks = new List<Task>();

            do
            {
                var resultSegment = container.GetBlobs().AsPages(continuationToken);

                foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        var blobClient = container.GetBlobClient(blobItem.Name);
                        string filename = Path.Combine(directory, blobItem.Name);

                        downloadTasks.Add(blobClient.DownloadToAsync(filename));
                    }
                    // Get the continuation token and loop until it is empty.
                    continuationToken = blobPage.ContinuationToken;
                }

            } while (continuationToken != "");

            await Task.WhenAll(downloadTasks);

            //Console.WriteLine("Download complete.");
        }
        // </DownloadResults>

        /// <summary>
        /// Deletes the jobs, assets and potentially the content key policy that were created.
        /// Generally, you should clean up everything except objects 
        /// that you are planning to reuse (typically, you will reuse Transforms, and you will persist output assets and StreamingLocators).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="accountName"></param>
        /// <param name="transformName"></param>
        /// <param name="jobName"></param>
        /// <param name="assetNames"></param>
        /// <param name="contentKeyPolicyName"></param>
        /// <returns></returns>
        // <CleanUp>
        private static async Task CleanUpAsync(
           IAzureMediaServicesClient client,
           string resourceGroupName,
           string accountName,
           string transformName,
           string jobName,
           List<string> assetNames,
           string contentKeyPolicyName = null
           )
        {
            await client.Jobs.DeleteAsync(resourceGroupName, accountName, transformName, jobName);

            foreach (var assetName in assetNames)
            {
                await client.Assets.DeleteAsync(resourceGroupName, accountName, assetName);
            }

            if (contentKeyPolicyName != null)
            {
                client.ContentKeyPolicies.Delete(resourceGroupName, accountName, contentKeyPolicyName);
            }
        }


        // </CleanUp>

    }
}
