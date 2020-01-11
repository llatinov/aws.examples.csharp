using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using SqsReader.Sqs.Models;

namespace SqsReader.Services.Processors
{
    public class ActorMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<ActorMessageProcessor> _logger;

        public ActorMessageProcessor(ILogger<ActorMessageProcessor> logger)
        {
            _logger = logger;
        }

        public bool CanProcess(string messageType)
        {
            return messageType == typeof(Actor).Name;
        }

        public void Process(Message message)
        {
            _logger.LogInformation($"ActorMessageProcessor invoked with: {message.Body}");
        }
    }
}