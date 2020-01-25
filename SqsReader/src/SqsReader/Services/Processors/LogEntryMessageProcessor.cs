using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Models;

namespace SqsReader.Services.Processors
{
    public class LogEntryMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<LogEntryMessageProcessor> _logger;

        public LogEntryMessageProcessor(ILogger<LogEntryMessageProcessor> logger)
        {
            _logger = logger;
        }

        public bool CanProcess(string messageType)
        {
            return messageType == typeof(LogEntry).Name;
        }

        public Task ProcessAsync(Message message)
        {
            _logger.LogInformation($"LogEntryMessageProcessor invoked with: {message.Body}");
            return Task.CompletedTask;
        }
    }
}