using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqsReader.Sqs;

namespace SqsReader.HealthChecks
{
    public class SqsHealthCheck : IHealthCheck
    {
        private readonly ISqsClient _sqsClient;

        public SqsHealthCheck(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var queueStatus = await _sqsClient.GetQueueStatusAsync();
            var healthStatus = queueStatus.IsHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            var description = $"Status for '{_sqsClient.GetQueueName()}' queue";

            return new HealthCheckResult(healthStatus, description);
        }
    }
}