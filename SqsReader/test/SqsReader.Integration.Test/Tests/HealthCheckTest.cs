using System.Net;
using System.Threading.Tasks;
using Moq;
using SqsReader.Sqs;
using Xunit;

namespace SqsReader.Integration.Test.Tests
{
    public class HealthCheckTest : BaseTest
    {
        [Fact]
        public async Task GetHealthReport_ReturnsHealthy()
        {
            SqsClientMock.Setup(x => x.GetQueueStatusAsync())
                .ReturnsAsync(new SqsStatus { IsHealthy = true });

            var response = await HealthCheckClient.GetHealth();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", response.Result.Status);
        }

        [Fact]
        public async Task GetHealthReport_ReturnsUnhealthy()
        {
            SqsClientMock.Setup(x => x.GetQueueStatusAsync())
                .ReturnsAsync(new SqsStatus { IsHealthy = false });

            var response = await HealthCheckClient.GetHealth();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", response.Result.Status);
        }
    }
}
