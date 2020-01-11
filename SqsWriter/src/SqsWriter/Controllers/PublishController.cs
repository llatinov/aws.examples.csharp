using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqsWriter.Sqs;
using SqsWriter.Sqs.Models;

namespace SqsWriter.Controllers
{
    [Route("api/[controller]")]
    public class PublishController : Controller
    {
        private readonly ISqsClient _sqsClient;

        public PublishController(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;
        }

        [HttpPost]
        [Route("movie")]
        public async Task<IActionResult> PublishMovie([FromBody]Movie movie)
        {
            await _sqsClient.PostMessageAsync(JsonConvert.SerializeObject(movie), typeof(Movie).Name);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost]
        [Route("actor")]
        public async Task<IActionResult> PublishActor([FromBody]Actor actor)
        {
            await _sqsClient.PostMessageAsync(JsonConvert.SerializeObject(actor), typeof(Actor).Name);
            return StatusCode((int)HttpStatusCode.Created);
        }
    }
}
