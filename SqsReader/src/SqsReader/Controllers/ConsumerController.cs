using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SqsReader.Services;

namespace SqsReader.Controllers
{
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly ISqsConsumerService _sqsConsumerService;

        public ConsumerController(ISqsConsumerService sqsConsumerService)
        {
            _sqsConsumerService = sqsConsumerService;
        }

        [HttpPost]
        [Route("start")]
        public async Task<IActionResult> Start()
        {
            await _sqsConsumerService.StartConsumingAsync();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("stop")]
        public async Task<IActionResult> Stop()
        {
            await _sqsConsumerService.StopConsumingAsync();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("reprocess")]
        public async Task<IActionResult> Reprocess()
        {
            await _sqsConsumerService.ReprocessMessagesAsync();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> Status()
        {
            var status = await _sqsConsumerService.GetStatusAsync();
            return StatusCode((int)HttpStatusCode.OK, status);
        }
    }
}
