using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using SqsWriter.Sqs;

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
            _logger.LogInformation("New Movie published with {Title}, {@Content}",
                 movie.Title, movie);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost]
        [Route("actor")]
        public async Task<IActionResult> PublishActor([FromBody]Actor actor)
        {
            await _sqsClient.PostMessageAsync(actor);
            _logger.LogInformation("New Actor published with {FirstName} and {LastName}, {@Content}",
                actor.FirstName, actor.LastName, actor);
            return StatusCode((int)HttpStatusCode.Created);
        }
    }
}
