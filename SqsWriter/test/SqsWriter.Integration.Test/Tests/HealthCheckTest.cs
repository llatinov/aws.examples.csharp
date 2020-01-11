using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqsWriter.Sqs;

namespace SqsWriter.Integration.Test.Tests
{
    [TestClass]
    public class HealthCheckTest : BaseTest
    {
        [TestMethod]
        public async Task GetHealthReport_ReturnsHealthy()
        {
            SqsClientMock.Setup(x => x.GetQueueStatus())
                .ReturnsAsync(new SqsStatus { IsHealthy = true });

            var response = await HealthCheckClient.GetHealth();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Healthy", response.Result.Status);
        }

        [TestMethod]
        public async Task GetHealthReport_ReturnsUnhealthy()
        {
            SqsClientMock.Setup(x => x.GetQueueStatus())
                .ReturnsAsync(new SqsStatus { IsHealthy = false });

            var response = await HealthCheckClient.GetHealth();

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.AreEqual("Unhealthy", response.Result.Status);
        }
    }
}
