using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqsWriter.Sqs;
using SqsWriter.Sqs.Models;

namespace SqsWriter.Controllers
{
    [Route("api/[controller]")]
    public class PublishController : Controller
    {
        private readonly ISqsClient _sqsClient;
        private readonly ILogger<PublishController> _logger;


        public PublishController(ISqsClient sqsClient, ILogger<PublishController> logger)
        {
            _sqsClient = sqsClient;
            _logger = logger;
        }

        [HttpPost]
        [Route("movie")]
        public async Task<IActionResult> PublishMovie([FromBody]Movie movie)
        {
            await _sqsClient.PostMessageAsync(movie);
            _logger.LogDebug("New Movie published with {Body}", JsonConvert.SerializeObject(movie));
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost]
        [Route("actor")]
        public async Task<IActionResult> PublishActor([FromBody]Actor actor)
        {
            await _sqsClient.PostMessageAsync(actor);
            _logger.LogDebug("New Actor published with {Body}", JsonConvert.SerializeObject(actor));
            return StatusCode((int)HttpStatusCode.Created);
        }
    }
}
