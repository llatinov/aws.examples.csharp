using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SqsWriter.Integration.Test.Client
{
    public class BaseClient
    {
        private readonly HttpClient _httpClient;

        protected BaseClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string path, object content = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri($"{_httpClient.BaseAddress}{path}")
            };

            if (content != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var value = await response.Content.ReadAsStringAsync();
            var result = new ApiResponse<T>
            {
                StatusCode = response.StatusCode,
                ResultAsString = value
            };

            try
            {
                result.Result = JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                // Nothing to do
            }

            return result;
        }
    }
}