using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SqsReader.Controllers;
using SqsReader.Services;
using SqsReader.Sqs;
using Xunit;

namespace SqsReader.Test.Controllers
{
    public class ConsumerControllerTest
    {
        [Theory, AutoNSubstituteData]
        public void Start_ReturnsCorrectResult_AndCallsCorrectMethod(
            [Frozen] ISqsConsumerService consumerServiceMock,
            ConsumerController controllerUnderTest)
        {
            var result = controllerUnderTest.Start();
            var asObjectResult = (StatusCodeResult)result;

            Assert.Equal(200, asObjectResult.StatusCode);
            consumerServiceMock.Received().StartConsuming();
        }

        [Theory, AutoNSubstituteData]
        public void Stop_ReturnsCorrectResult_AndCallsCorrectMethod(
            [Frozen] ISqsConsumerService consumerServiceMock,
            ConsumerController controllerUnderTest)
        {
            var result = controllerUnderTest.Stop();
            var asObjectResult = (StatusCodeResult)result;

            Assert.Equal(200, asObjectResult.StatusCode);
            consumerServiceMock.Received().StopConsuming();
        }

        [Theory, AutoNSubstituteData]
        public void Reprocess_ReturnsCorrectResult_AndCallsCorrectMethod(
            [Frozen] ISqsConsumerService consumerServiceMock,
            ConsumerController controllerUnderTest)
        {
            var result = controllerUnderTest.Reprocess();
            var asObjectResult = (StatusCodeResult)result;

            Assert.Equal(200, asObjectResult.StatusCode);
            consumerServiceMock.Received().ReprocessMessages();
        }

        [Theory, AutoNSubstituteData]
        public async Task Status_ReturnsCorrectResult_AndCallsCorrectMethod(
            [Frozen] ISqsConsumerService consumerServiceMock,
            ConsumerController controllerUnderTest)
        {
            var sqsStatus = new SqsStatus();
            consumerServiceMock.GetStatus().Returns(sqsStatus);

            var result = await controllerUnderTest.Status();
            var asObjectResult = (ObjectResult)result;
            var asObjectValue = (SqsStatus)asObjectResult.Value;

            Assert.Equal(200, asObjectResult.StatusCode);
            Assert.Equal(sqsStatus, asObjectValue);
            await consumerServiceMock.Received().GetStatus();
        }
    }
}