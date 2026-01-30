using Core.Storage.Interfaces;
using Idata.Data;
using Ihelpers.Helpers;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Core.Storage
{
    public class MediaStorage : IStorageBase
    {
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private readonly IdataContext _dbContext;

        public MediaStorage(IdataContext dbContext)
        {
            _apiUrl = ConfigurationHelper.GetConfig($"Reporting:{ConfigurationHelper.GetConfig("Reporting:ActiveEnvironment")}:Url");
            _httpClient = new HttpClient(); // Idealmente inyectar IHttpClientFactory, pero uso constructor simple por restricción de firma
            _dbContext = dbContext;
        }

        public async Task<string> CreateFile(string fileName, Stream fileStream, UrlRequestBase request)
        {
            try
            {
                var uploadUrl = $"{_apiUrl}api/media/v1/files/upload";

                using (var content = new MultipartFormDataContent())
                {
                    // 1. Agregar el archivo al contenido multipart
                    // Clonamos el stream o nos aseguramos que esté en posición 0
                    if (fileStream.Position > 0 && fileStream.CanSeek) fileStream.Position = 0;

                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(streamContent, "file", fileName);

                    // 2. Serializar el objeto UrlRequestBase (Opción A)
                    var serializeOptions = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    //Add the jsonToken to  the _hhtpClient
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.currentContextToken);


                    var jsonRequest = JsonConvert.SerializeObject(request, settings: serializeOptions);
                    content.Add(new StringContent(jsonRequest), "request");

                    // 3. Llamada al API de Media
                    var response = await _httpClient.PostAsync(uploadUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error uploading file to Media API: {response.ReasonPhrase}");
                    }

                    // Retorna la URL generada por el API de Media
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                // Manejo básico de excepción para no romper el flujo inesperadamente
                throw new Exception($"MediaStorage Upload Failed: {ex.Message}", ex);
            }
        }

        public async Task<Stream?> DownloadFile(string fileName)
        {
            // fileName aquí viene como la URL completa: "https://api-media.../download/{GUID}"
            try
            {
                // Hacemos el GET directo a la URL proporcionada
                var response = await _httpClient.GetAsync(fileName);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Retornamos el stream directo de la respuesta HTTP
                return await response.Content.ReadAsStreamAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<Dictionary<string, object>?> ReadFile(string fileName, UrlRequestBase request)
        {
            try
            {
                var getAllFilesUrl = $"{_apiUrl}api/media/v1/files/readFileProps/{fileName}";

                using (var content = new MultipartFormDataContent())
                {


                    var serializeOptions = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.currentContextToken);


                    var jsonRequest = JsonConvert.SerializeObject(request, settings: serializeOptions);

                    content.Add(new StringContent(jsonRequest), "request");

                    var response = await _httpClient.PostAsync(getAllFilesUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error uploading file to Media API: {response.ReasonPhrase}");
                    }


                    var initialResponse = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<Dictionary<string, object>?>(initialResponse, settings: serializeOptions);

                }
            }
            catch (Exception ex)
            {
                // Manejo básico de excepción para no romper el flujo inesperadamente
                throw new Exception($"MediaStorage Upload Failed: {ex.Message}", ex);
            }
            // Implementación opcional: Podría llamar a un endpoint de metadata si existiera
            throw new NotImplementedException();
        }


        public Task<bool> RemoveFile(string fileName)
        {
            // Implementación opcional: Podría llamar a endpoint delete
            throw new NotImplementedException();
        }

        public async Task<List<Dictionary<string, object>>> GetAllUserFiles(string userId, UrlRequestBase request)
        {


            try
            {
                var getAllFilesUrl = $"{_apiUrl}api/media/v1/files/getUserFiles/{userId}";

                using (var content = new MultipartFormDataContent())
                {


                    var serializeOptions = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.currentContextToken);


                    var jsonRequest = JsonConvert.SerializeObject(request, settings: serializeOptions);
                    content.Add(new StringContent(jsonRequest), "request");

                    var response = await _httpClient.PostAsync(getAllFilesUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error uploading file to Media API: {response.ReasonPhrase}");
                    }


                    var initialResponse = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(initialResponse, settings: serializeOptions);

                }
            }
            catch (Exception ex)
            {
                // Manejo básico de excepción para no romper el flujo inesperadamente
                throw new Exception($"MediaStorage Upload Failed: {ex.Message}", ex);
            }
        }
    }
}
