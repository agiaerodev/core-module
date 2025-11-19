using Azure.Storage.Blobs;
using Core.Exceptions;
using Core.Storage.Interfaces;
using Ihelpers.Helpers;
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
        const string ConfigKey = "AzureBlobStorage:DefaultConnection";

        /// <summary>
        /// The name of the container in Azure Blob Storage.
        /// </summary>
        const string ContainerName = "agione";

        /// <summary>
        /// Creates a new instance of the `AzureStorageBase` class.
        /// </summary>

        public AzureStorageBase()
        {
            // Get the default connection string from the configuration.
            string? conString = ConfigurationHelper.GetConfig(ConfigKey);

            // If the connection string is not found, throw an exception.
            if (conString == null) throw new Exception($"AzureStorageBase configuration with key {0} was not found");

            BlobClientOptions options = new BlobClientOptions();

            options.Retry.NetworkTimeout = Timeout.InfiniteTimeSpan;

            // Create a `BlobServiceClient` using the connection string.
            blobServiceClient = new BlobServiceClient(conString, options: options);

            // Get the blob container client for the container with the specified name.
            blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            // If the container does not exist, create it.
            if (!blobContainerClient.Exists())
            {
                blobContainerClient = blobServiceClient.CreateBlobContainer(ContainerName);
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
        /// Reads a file from Azure Blob Storage and returns its properties.
        /// </summary>
        /// <param name="fileName">The name of the file to be read.</param>
        /// <returns>A dictionary containing the file's properties such as last modified time, path, size, and file format.</returns>
        public async Task<Dictionary<string, object>?> ReadFile(string fileName)
        {
            // Initialize the result variable as null
            Dictionary<string, object>? result = null;
            try
            {
                // Get the BlobClient instance for the file
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName.ToLower());

                // Check if the file exists in the Blob Storage
                bool fileExists = await blobClient.ExistsAsync();

                // Get the download endpoint from the app settings configuration file
                string? downloadEndpoint = Ihelpers.Helpers.ConfigurationHelper.GetConfig("DefaultConfigs:ReportsEndpoint");
                if (downloadEndpoint == null)
                {
                    // Throw an exception if the download endpoint is not found
                    throw new Exception("Reports Endpoint configuration not found on app settings file");
                }

                if (fileExists)
                {
                    // Get the last modified time of the file
                    var lastModified = blobClient.GetProperties().Value.LastModified.UtcDateTime;

                    // Initialize the result dictionary with the file properties
                    result = new Dictionary<string, object>
            {
                { "lastModified", lastModified },
                { "path", blobClient.Uri + "?lastModified=" + System.Web.HttpUtility.UrlEncode(lastModified.ToString()) },
                { "size", blobClient.GetProperties().Value.ContentLength },
                { "fileFormat", fileName.Split('.')[1] }
            };

                    // Return the result dictionary
                    return result;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception and log the error message
                ExceptionBase.HandleException(ex, $"Error reading {fileName} from azure storage");
            }

            // Return the result
            return result;
        }
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

            if (fileName.Contains("http")) {
                fileName = fileName.Replace(blobContainerClient.Uri.ToString(),"");
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
    }
}
