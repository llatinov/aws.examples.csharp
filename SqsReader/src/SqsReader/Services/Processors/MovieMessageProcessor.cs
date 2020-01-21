using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Models;

namespace SqsReader.Services.Processors
{
    public class MovieMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MovieMessageProcessor> _logger;

        public MovieMessageProcessor(ILogger<MovieMessageProcessor> logger)
        {
            _logger = logger;
        }

        public bool CanProcess(string messageType)
        {
            return messageType == typeof(Movie).Name;
        }

        public Task ProcessAsync(Message message)
        {
            _logger.LogInformation($"MovieMessageProcessor invoked with: {message.Body}");
            return Task.CompletedTask;
        }
    }
}