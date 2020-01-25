using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;

namespace SqsWriter.Sqs
{
    public class SqsClient : ISqsClient
    {
        private readonly AppConfig _appConfig;
        private readonly IAmazonSQS _sqsClient;
        private readonly ILogger<SqsClient> _logger;
        private readonly ConcurrentDictionary<string, string> _queueUrlCache;

        public SqsClient(IOptions<AppConfig> awsConfig, IAmazonSQS sqsClient, ILogger<SqsClient> logger)
        {
            _appConfig = awsConfig.Value;
            _sqsClient = sqsClient;
            _logger = logger;
            _queueUrlCache = new ConcurrentDictionary<string, string>();
        }

        public string GetQueueName()
        {
            return _appConfig.AwsQueueName;
        }

        public async Task CreateQueueAsync()
        {
            const string arnAttribute = "QueueArn";

            try
            {
                var createQueueRequest = new CreateQueueRequest();
                if (_appConfig.AwsQueueIsFifo)
                {
                    createQueueRequest.Attributes.Add("FifoQueue", "true");
                }

                createQueueRequest.QueueName = _appConfig.AwsQueueName;
                var createQueueResponse = await _sqsClient.CreateQueueAsync(createQueueRequest);
                createQueueRequest.QueueName = _appConfig.AwsDeadLetterQueueName;
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
                        {"ReceiveMessageWaitTimeSeconds", _appConfig.AwsQueueLongPollTimeSeconds.ToString()}
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when creating SQS queue {_appConfig.AwsQueueName} and {_appConfig.AwsDeadLetterQueueName}");
            }
        }

        public async Task<SqsStatus> GetQueueStatusAsync()
        {
            var queueName = _appConfig.AwsQueueName;
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var attributes = new List<string> { "ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible", "LastModifiedTimestamp" };
                var response = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueUrl, attributes));

                return new SqsStatus
                {
                    IsHealthy = response.HttpStatusCode == HttpStatusCode.OK,
                    Region = _appConfig.AwsRegion,
                    QueueName = queueName,
                    LongPollTimeSeconds = _appConfig.AwsQueueLongPollTimeSeconds,
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
                    WaitTimeSeconds = _appConfig.AwsQueueLongPollTimeSeconds,
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
            return await GetMessagesAsync(_appConfig.AwsQueueName, cancellationToken);
        }

        public async Task PostMessageAsync<T>(string queueName, T message)
        {
            var queueUrl = await GetQueueUrl(queueName);

            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = JsonConvert.SerializeObject(message),
                    MessageAttributes = SqsMessageTypeAttribute.CreateAttributes<T>()
                };
                if (_appConfig.AwsQueueIsFifo)
                {
                    sendMessageRequest.MessageGroupId = typeof(T).Name;
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

        public async Task PostMessageAsync<T>(T message)
        {
            await PostMessageAsync(_appConfig.AwsQueueName, message);
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