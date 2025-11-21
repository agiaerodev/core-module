using Core.Exceptions;
using Core.Services.Interfaces;
using Ihelpers.Helpers;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Core.Services
{
    public class ReportingService : IReportingService
    {

        private readonly HttpClient _httpClient;

        public ReportingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateExcelFile(string url) {
            string responseString = "";
            var baseEndpoint = ConfigurationHelper.GetConfig<string>("FileGeneratorService:Endpoint");
            try
            {
                if (baseEndpoint != null) {
                var body = new
                {
                    url = HttpUtility.UrlDecode(url)

                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(baseEndpoint + "/api/v1/excel/url", content);
                    responseString = response.ToString();
                    response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                string? result = JsonDocument.Parse(json)
                    .RootElement.GetProperty("url").GetString();


                url = result ?? url;
            }

            }
            catch (Exception ex)
            {
                Core.Exceptions.ExceptionBase.HandleSilentException(ex, $"Error converting file to excel, response from creator: {responseString}");
            }

            return url; 
        }
    }
}
