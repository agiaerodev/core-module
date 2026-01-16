using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Exceptions;
using Core.Storage.Interfaces;
using Idata.Data;
using Ihelpers.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Core.Storage
{
    /// <summary>
    /// The `AzureStorageBase` class provides an implementation of the `IStorageBase` interface that is backed by Azure Blob Storage.
    /// </summary>
    public class AzureStorageBase : IStorageBase
    {
        private BlobServiceClient blobServiceClient;

        public BlobContainerClient blobContainerClient;

        private BlobClient blobClient;

        /// <summary>
        /// The configuration key for the default connection string for the Azure Blob Storage.
        /// </summary>
        const string ConfigKey = "BlobStorage:DefaultConnection";

  

        /// <summary>
        /// Creates a new instance of the `AzureStorageBase` class.
        /// </summary>

        public AzureStorageBase()
        {


            var tenantId = ConfigurationHelper.GetConfig("BlobStorage:TenantId", useCache: true);
            var clientId = ConfigurationHelper.GetConfig("BlobStorage:ClientId", useCache: true);
            var clientSecret = ConfigurationHelper.GetConfig("BlobStorage:ClientSecret", useCache: true);
            var containerName = ConfigurationHelper.GetConfig("BlobStorage:ContainerName", useCache: true);
            var accountName = ConfigurationHelper.GetConfig("BlobStorage:AccountName", useCache: true);



            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var serviceUri = new Uri($"https://{accountName}.blob.core.windows.net");

           
            BlobClientOptions options = new BlobClientOptions();

            options.Retry.NetworkTimeout = Timeout.InfiniteTimeSpan;

            // Create a `BlobServiceClient` using the connection string.
            var blobServiceClient = new BlobServiceClient(serviceUri, credential);

            // Get the blob container client for the container with the specified name.
            blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);


            // If the container does not exist, create it.
            if (!blobContainerClient.Exists())
            {
                blobContainerClient = blobServiceClient.CreateBlobContainer(containerName);
            }
        }


        /// <summary>
        /// Creates a file in Azure Blob storage.
        /// </summary>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <param name="fileStream">The stream of the file to be created.</param>
        public async Task<string> CreateFile(string fileName, Stream fileStream, UrlRequestBase? request)
        {
            try
            {
                fileName = fileName.Replace("/", "");
                fileName = request != null ? $"user={request?.currentContextUser?.id}/{fileName}" : fileName;
                // Create a BlobClient instance
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName.ToLower());

                // Check if the file exists and delete it if it does
                blobClient.DeleteIfExists();

                // Reset the position of the stream to the beginning
                fileStream.Position = 0;

                // Upload the file to the blob container
                var response = await blobClient.UploadAsync(fileStream);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the file creation process
                ExceptionBase.HandleException(ex, $"Error reading {fileName} from azure storage");

                return null;
            }
        }


        /// <summary>
       
        /// <summary>
        /// Downloads the specified file from the Blob storage.
        /// </summary>
        /// <param name="fileName">The name of the file to be downloaded.</param>
        /// <returns>A stream of the downloaded file.</returns>
        public async Task<Stream?> DownloadFile(string fileName)
        {
            // Get a reference to the blob client for the specified file
            BlobClient blobClient = blobContainerClient.GetBlobClient(fileName.ToLower());

            Stream? response = null;

            // Check if the file exists in the Blob storage
            bool fileExists = await blobClient.ExistsAsync();

            // Get the download endpoint from the configuration
            string? downloadEndpoint = Ihelpers.Helpers.ConfigurationHelper.GetConfig("DefaultConfigs:ReportsEndpoint");

            // Throw an exception if the file does not exist
            if (!fileExists) throw new Exception($"Requested file not exists: {fileName}");

            // Throw an exception if the download endpoint is not found in the configuration
            if (downloadEndpoint == null) throw new Exception("ReportsEndpoint configuration not found on app settings file");

            try
            {
                // Create a memory stream to store the downloaded data
                Stream DownloadData = new MemoryStream();

                // Download the data to the memory stream
                blobClient.DownloadTo(DownloadData);

                // Set the position of the memory stream to the beginning
                DownloadData.Position = 0;

                // Return the memory stream
                return DownloadData;

            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the download
                ExceptionBase.HandleException(ex, $"Error downloading file {fileName}");
            }

            // Return the response
            return response;
        }


        public async Task<bool> RemoveFile(string fileName)
        {

            if (fileName.Contains("http"))
            {
                fileName = fileName.Replace(blobContainerClient.Uri.ToString(), "");
            }
            // Get a reference to the blob client for the specified file
            BlobClient blobClient = blobContainerClient.GetBlobClient((fileName));

            Stream? response = null;

            // Check if the file exists in the Blob storage
            bool fileExists = await blobClient.ExistsAsync();

            // Get the download endpoint from the configuration
            string? downloadEndpoint = Ihelpers.Helpers.ConfigurationHelper.GetConfig("DefaultConfigs:ReportsEndpoint");

            // Throw an exception if the file does not exist
            if (!fileExists) throw new Exception($"Requested file not exists: {fileName}");

            // Throw an exception if the download endpoint is not found in the configuration
            if (downloadEndpoint == null) throw new Exception("ReportsEndpoint configuration not found on app settings file");

            try
            {

                await blobClient.DeleteIfExistsAsync();


                // Return the memory stream
                //return true;

            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the download
                ExceptionBase.HandleException(ex, $"Error downloading file {fileName}");
                return false;
            }

            // Return the response
            return true;
        }


        public async Task<List<Dictionary<string, object>>> GetAllUserFiles(string userId, UrlRequestBase requestBase)
        {
            var reportFormats = Ihelpers.Helpers.ConfigurationHelper.GetConfig<string[]>("DefaultConfigs:ReportFormats");

            var response = new List<Dictionary<string, object>>();

            string prefix = $"user={userId}";


            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync(prefix: prefix))
            {

                bool isValidFormat = reportFormats.Any(format => blobItem.Name.EndsWith(format, StringComparison.OrdinalIgnoreCase));

                if (isValidFormat)
                {
                    var lastModified = blobItem.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow;

                    string blobUri = $"{blobContainerClient.Uri}/{blobItem.Name}";

                    var fileDetails = new Dictionary<string, object>
                    {
                        { "lastModified", lastModified },
                        { "path", $"{blobUri}?lastModified={System.Web.HttpUtility.UrlEncode(lastModified.ToString())}" },
                        { "size", blobItem.Properties.ContentLength },
                        { "fileFormat", blobItem.Name.Split('.').Last() },
                        { "fileName", blobItem.Name }
                    };

                    response.Add(fileDetails);
                }
            }

            return response;
        }
    }
}
