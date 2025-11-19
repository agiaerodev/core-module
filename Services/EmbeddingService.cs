using Core.Services.Interfaces;
using Ihelpers.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Core.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;

        public EmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var endpoint = ConfigurationHelper.GetConfig<string>("AzureOpenAI:Embeddings:Endpoint");
            var deployment = ConfigurationHelper.GetConfig<string>("AzureOpenAI:Embeddings:EmbeddingDeployment");
            var apiVersion = ConfigurationHelper.GetConfig<string>("AzureOpenAI:Embeddings:ApiVersion");
            var apiKey = ConfigurationHelper.GetConfig<string>("AzureOpenAI:Embeddings:ApiKey");

            var requestUri = $"{endpoint}/openai/deployments/{deployment}/embeddings?api-version={apiVersion}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var body = JsonSerializer.Serialize(new
            {
                input = text,
                model = deployment
            });

            var response = await _httpClient.PostAsync(requestUri, new StringContent(body, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var vector = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return vector;
        }
    }
}
