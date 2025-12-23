using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RusticaPortal_PRMVAN.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public async Task<T> PostAsync<T>(string endpoint, object body)
        {
            try
            {
                var url = $"{_baseUrl}{endpoint}";
                var json = body switch
                {
                    JsonElement element => element.GetRawText(),
                    string raw => raw,
                    _ => JsonConvert.SerializeObject(body)
                };
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (HttpRequestException ex)
            {
                // Aquí puedes loguear el error o devolver un mensaje amigable
                // throw new Exception($"No se pudo conectar con el API en {_baseUrl}. Detalle: {ex.Message}");
                throw new Exception($"No se pudo conectar, por favor inténtelo nuevamente");
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var url = $"{_baseUrl}{endpoint}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (HttpRequestException)
            {
                throw new Exception("No se pudo conectar, por favor inténtelo nuevamente");
            }
        }
    }
}
