using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using SqsReader.Services.Processors;
using SqsReader.Sqs;

namespace SqsReader.Services
{
    public class SqsConsumerService : ISqsConsumerService
    {
        private readonly ISqsClient _sqsClient;
        private readonly IEnumerable<IMessageProcessor> _messageProcessors;
        private readonly ILogger<SqsConsumerService> _logger;

        private CancellationTokenSource _tokenSource;

        public SqsConsumerService(ISqsClient sqsClient, IEnumerable<IMessageProcessor> messageProcessors, ILogger<SqsConsumerService> logger)
        {
            _sqsClient = sqsClient;
            _messageProcessors = messageProcessors;
            _logger = logger;
        }

        public async Task<SqsStatus> GetStatusAsync()
        {
            var status = await _sqsClient.GetQueueStatus();
            status.IsConsuming = IsConsuming();

            return status;
        }

        public async Task StartConsumingAsync()
        {
            if (IsConsuming())
                return;

            _tokenSource = new CancellationTokenSource();
            await ProcessAsync();
        }

        public Task StopConsumingAsync()
        {
            if (IsConsuming())
            {
                _tokenSource.Cancel();
            }
            return Task.CompletedTask;
        }

        public async Task ReprocessMessagesAsync()
        {
            await _sqsClient.RestoreFromDeadLetterQueue();
        }

        private bool IsConsuming()
        {
            return _tokenSource != null && !_tokenSource.Token.IsCancellationRequested;
        }

        private async Task ProcessAsync()
        {
            try
            {
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    var messages = await _sqsClient.GetMessagesAsync(_tokenSource.Token);
                    messages.ForEach(async x => await ProcessMessageAsync(x));
                }
            }
            catch (OperationCanceledException)
            {
                //operation has been canceled but it shouldn't be propagated
            }
        }

        private async Task ProcessMessageAsync(Message message)
        {
            try
            {
                var messageType = message.MessageAttributes.SingleOrDefault(x => x.Key == MessageAttributes.MessageType).Value;
                if (messageType == null)
                {
                    throw new Exception($"No '{MessageAttributes.MessageType}' attribute present in message");
                }

                var processor = _messageProcessors.SingleOrDefault(x => x.CanProcess(messageType.StringValue));
                if (processor == null)
                {
                    throw new Exception($"No processor found for message type '{messageType.StringValue}'");
                }

                await processor.ProcessAsync(message);
                await _sqsClient.DeleteMessageAsync(message.ReceiptHandle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cannot process message [id: {message.MessageId}, receiptHandle: {message.ReceiptHandle}, body: {message.Body}] from queue {_sqsClient.GetQueueName()}");
            }
        }
    }
}