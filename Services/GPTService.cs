using Core.Services.Interfaces;
using Ihelpers.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Core.Services
{
    public class GPTService : IGPTService
    {
        private readonly HttpClient _httpClient;

        public GPTService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateFormAsync(string prompt, string? systemPrompt = null)
        {
            var endpoint = ConfigurationHelper.GetConfig<string>("AzureOpenAI:GPTMini:Endpoint");
            var deployment = ConfigurationHelper.GetConfig<string>("AzureOpenAI:GPTMini:GptDeployment");
            var apiVersion = ConfigurationHelper.GetConfig<string>("AzureOpenAI:GPTMini:ApiVersion");
            var apiKey = ConfigurationHelper.GetConfig<string>("AzureOpenAI:GPTMini:ApiKey");

            var uri = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            systemPrompt = systemPrompt ?? @$"
                Eres un asistente que llena formularios. Extrae el nombre de la aerolinea, el codigo del aereopuerto y el numero de vuelo.
                en algunos casos el numero de vuelo puede venir como un fueling ticket number, por ejemplo: 264870
                Usa los siguientes valores como base:

                Prompt del usuario: {prompt}

                Devuelve un JSON:
                {{
                    ""airlineName"": """",
                    ""flightNumber"": """",
                    ""stationCode"": """"
                }}";

            var body = new
            {
                messages = new[]
                {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
                temperature = 0.2,
                model = deployment
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var resultText = JsonDocument.Parse(json)
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content").GetString();

            return resultText;
        }
    }
}
