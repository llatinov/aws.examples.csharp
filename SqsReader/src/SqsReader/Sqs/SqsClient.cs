using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SqsReader.Sqs
{
    public class SqsClient : ISqsClient
    {
        private readonly AppConfig.AwsConfig _awsConfig;
        private readonly IAmazonSQS _sqsClient;
        private readonly ILogger<SqsClient> _logger;
        private readonly ConcurrentDictionary<string, string> _queueUrlCache;

        public SqsClient(IOptions<AppConfig> awsConfig, IAmazonSQS sqsClient, ILogger<SqsClient> logger)
        {
            _awsConfig = awsConfig.Value.AwsSettings;
            _sqsClient = sqsClient;
            _logger = logger;
            _queueUrlCache = new ConcurrentDictionary<string, string>();
        }

        public string GetQueueName()
        {
            return _awsConfig.QueueName;
        }

        public async Task CreateQueue()
        {
            const string arnAttribute = "QueueArn";

            try
            {
                var createQueueRequest = new CreateQueueRequest();
                if (_awsConfig.IsFifo)
                {
                    createQueueRequest.Attributes.Add("FifoQueue", "true");
                }

                createQueueRequest.QueueName = _awsConfig.QueueName;
                var createQueueResponse = await _sqsClient.CreateQueueAsync(createQueueRequest);
                createQueueRequest.QueueName = _awsConfig.DeadLetterQueueName;
                var createDeadLetterQueueResponse = await _sqsClient.CreateQueueAsync(createQueueRequest);

                // Get the the ARN of dead letter queue and configure main queue to deliver messages to it
                var attributes = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
                {
                    QueueUrl = createDeadLetterQueueResponse.QueueUrl,
                    AttributeNames = new List<string> { arnAttribute }
                });
                var deadLetterQueueArn = attributes.Attributes[arnAttribute];

                // RedrivePolicy on main queue to deliver messages to dead letter queue if they fail processing after 3 times
                var redrivePolicy = new
                {
                    maxReceiveCount = "3",
                    deadLetterTargetArn = deadLetterQueueArn
                };
                await _sqsClient.SetQueueAttributesAsync(new SetQueueAttributesRequest
                {
                    QueueUrl = createQueueResponse.QueueUrl,
                    Attributes = new Dictionary<string, string>
                    {
                        {"RedrivePolicy", JsonConvert.SerializeObject(redrivePolicy)},
                        // Enable Long polling
                        {"ReceiveMessageWaitTimeSeconds", _awsConfig.LongPollTimeSeconds.ToString()}
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when creating SQS queue {_awsConfig.QueueName} and {_awsConfig.DeadLetterQueueName}");
            }
        }

        public async Task<SqsStatus> GetQueueStatus()
        {
            var queueName = _awsConfig.QueueName;
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var attributes = new List<string> { "ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible", "LastModifiedTimestamp" };
                var response = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueUrl, attributes));

                return new SqsStatus
                {
                    IsHealthy = response.HttpStatusCode == HttpStatusCode.OK,
                    Region = _awsConfig.AwsRegion,
                    QueueName = queueName,
                    LongPollTimeSeconds = _awsConfig.LongPollTimeSeconds,
                    ApproximateNumberOfMessages = response.ApproximateNumberOfMessages,
                    ApproximateNumberOfMessagesNotVisible = response.ApproximateNumberOfMessagesNotVisible,
                    LastModifiedTimestamp = response.LastModifiedTimestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to GetNumberOfMessages for queue {queueName}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = _awsConfig.LongPollTimeSeconds,
                    AttributeNames = new List<string> { "ApproximateReceiveCount" },
                    MessageAttributeNames = new List<string> { "*" }
                }, cancellationToken);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AmazonSQSException($"Failed to GetMessagesAsync for queue {queueName}. Response: {response.HttpStatusCode}");
                }

                return response.Messages;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning($"Failed to GetMessagesAsync for queue {queueName} because the task was canceled");
                return new List<Message>();
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to GetMessagesAsync for queue {queueName}");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(CancellationToken cancellationToken = default)
        {
            return await GetMessagesAsync(_awsConfig.QueueName, cancellationToken);
        }

        public async Task PostMessageAsync(string queueName, string messageBody, string messageType)
        {
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = messageBody,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            MessageAttributes.MessageType, new MessageAttributeValue
                            {
                                DataType = nameof(String),
                                StringValue = messageType
                            }
                        }
                    }
                };
                if (_awsConfig.IsFifo)
                {
                    sendMessageRequest.MessageGroupId = messageType;
                    sendMessageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
                }

                await _sqsClient.SendMessageAsync(sendMessageRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to PostMessagesAsync to queue '{queueName}'. Exception: {ex.Message}");
                throw;
            }
        }

        public async Task PostMessageAsync(string messageBody, string messageType)
        {
            await PostMessageAsync(_awsConfig.QueueName, messageBody, messageType);
        }

        public async Task DeleteMessageAsync(string queueName, string receiptHandle)
        {
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var response = await _sqsClient.DeleteMessageAsync(queueUrl, receiptHandle);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AmazonSQSException($"Failed to DeleteMessageAsync with for [{receiptHandle}] from queue '{queueName}'. Response: {response.HttpStatusCode}");
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to DeleteMessageAsync from queue {queueName}");
                throw;
            }
        }

        public async Task DeleteMessageAsync(string receiptHandle)
        {
            await DeleteMessageAsync(_awsConfig.QueueName, receiptHandle);
        }

        public async Task RestoreFromDeadLetterQueue(CancellationToken cancellationToken = default)
        {
            var deadLetterQueueName = _awsConfig.DeadLetterQueueName;

            try
            {
                var token = new CancellationTokenSource();
                while (!token.Token.IsCancellationRequested)
                {
                    var messages = await GetMessagesAsync(deadLetterQueueName, cancellationToken);
                    if (!messages.Any())
                    {
                        token.Cancel();
                        continue;
                    }

                    messages.ForEach(async message =>
                    {
                        var messageType = message.MessageAttributes.GetMessageType();
                        if (messageType != null)
                        {
                            await PostMessageAsync(message.Body, messageType);
                            await DeleteMessageAsync(deadLetterQueueName, message.ReceiptHandle);
                        }
                    });
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to ReprocessMessages from queue {deadLetterQueueName}");
                throw;
            }
        }

        private async Task<string> GetQueueUrl(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentException("Queue name should not be blank.");
            }

            if (_queueUrlCache.TryGetValue(queueName, out var result))
            {
                return result;
            }

            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(queueName);
                return _queueUrlCache.AddOrUpdate(queueName, response.QueueUrl, (q, url) => url);
            }
            catch (QueueDoesNotExistException ex)
            {
                throw new InvalidOperationException($"Could not retrieve the URL for the queue '{queueName}' as it does not exist or you do not have access to it.", ex);
            }
        }
    }
}