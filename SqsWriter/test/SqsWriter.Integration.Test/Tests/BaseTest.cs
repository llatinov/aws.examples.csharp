using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using SqsWriter.Integration.Test.Client;
using SqsWriter.Sqs;

namespace SqsWriter.Integration.Test.Tests
{
    public abstract class BaseTest
    {
        protected Mock<ISqsClient> SqsClientMock;
        protected PublishClient PublishClient;
        protected HealthCheckClient HealthCheckClient;

        protected BaseTest()
        {
            SqsClientMock = new Mock<ISqsClient>();

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseSerilog()
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(SqsClientMock.Object);
                }));

            var httpClient = server.CreateClient();
            PublishClient = new PublishClient(httpClient);
            HealthCheckClient = new HealthCheckClient(httpClient);
        }
    }
}