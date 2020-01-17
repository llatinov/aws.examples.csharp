using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using SqsReader.Integration.Test.Client;
using SqsReader.Services;
using SqsReader.Sqs;

namespace SqsReader.Integration.Test.Tests
{
    public abstract class BaseTest : IDisposable
    {
        protected Mock<ISqsClient> SqsClientMock;
        protected Mock<ISqsConsumerService> SqsConsumerServiceMock;
        protected HealthCheckClient HealthCheckClient;

        protected BaseTest()
        {
            SqsClientMock = new Mock<ISqsClient>();
            SqsConsumerServiceMock = new Mock<ISqsConsumerService>();

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseSerilog()
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(SqsClientMock.Object);
                    services.AddSingleton(SqsConsumerServiceMock.Object);
                }));

            var httpClient = server.CreateClient();
            HealthCheckClient = new HealthCheckClient(httpClient);
        }

        public void Dispose()
        {
            SqsClientMock.Verify(x => x.CreateQueue());
            SqsConsumerServiceMock.Verify(x => x.StartConsumingAsync());
        }
    }
}