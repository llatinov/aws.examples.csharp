using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqsReader.Controllers;
using SqsReader.Services;
using SqsReader.Sqs;

namespace SqsReader.Test.Controllers
{
    [TestClass]
    public class ConsumerControllerTest
    {
        private ConsumerController _consumerControllerUnderTest;
        private Mock<ISqsConsumerService> _consumerServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _consumerServiceMock = new Mock<ISqsConsumerService>();
            _consumerControllerUnderTest = new ConsumerController(_consumerServiceMock.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _consumerServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Start_ReturnsCorrectResult_AndCallsCorrectMethod()
        {
            var result = _consumerControllerUnderTest.Start();
            var asObjectResult = (StatusCodeResult)result;

            Assert.AreEqual(200, asObjectResult.StatusCode);
            _consumerServiceMock.Verify(x => x.StartConsuming());
        }

        [TestMethod]
        public void Stop_ReturnsCorrectResult_AndCallsCorrectMethod()
        {
            var result = _consumerControllerUnderTest.Stop();
            var asObjectResult = (StatusCodeResult)result;

            Assert.AreEqual(200, asObjectResult.StatusCode);
            _consumerServiceMock.Verify(x => x.StopConsuming());
        }

        [TestMethod]
        public void Reprocess_ReturnsCorrectResult_AndCallsCorrectMethod()
        {
            var result = _consumerControllerUnderTest.Reprocess();
            var asObjectResult = (StatusCodeResult)result;

            Assert.AreEqual(200, asObjectResult.StatusCode);
            _consumerServiceMock.Verify(x => x.ReprocessMessages());
        }

        [TestMethod]
        public async Task Status_ReturnsCorrectResult_AndCallsCorrectMethod()
        {
            var sqsStatus = new SqsStatus();
            _consumerServiceMock.Setup(x => x.GetStatus())
                .ReturnsAsync(sqsStatus);

            var result = await _consumerControllerUnderTest.Status();
            var asObjectResult = (ObjectResult)result;
            var asObjectValue = (SqsStatus)asObjectResult.Value;

            Assert.AreEqual(200, asObjectResult.StatusCode);
            Assert.AreEqual(sqsStatus, asObjectValue);
            _consumerServiceMock.Verify(x => x.GetStatus());
        }
    }
}
