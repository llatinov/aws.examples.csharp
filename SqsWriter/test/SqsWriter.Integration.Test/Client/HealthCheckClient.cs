using System.Net.Http;
using System.Threading.Tasks;
using SqsWriter.HealthChecks;

namespace SqsWriter.Integration.Test.Client
{
    public class HealthCheckClient : BaseClient
    {
        public HealthCheckClient(HttpClient httpClient)
            : base(httpClient) { }

        public async Task<ApiResponse<HealthResult>> GetHealth()
        {
            return await SendAsync<HealthResult>(HttpMethod.Get, "health");
        }
    }
}