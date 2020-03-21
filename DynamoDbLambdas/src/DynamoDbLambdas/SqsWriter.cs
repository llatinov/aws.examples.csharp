using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Models;
using Newtonsoft.Json;

namespace DynamoDbLambdas
{
    public class SqsWriter : ISqsWriter
    {
        private static readonly string QueueName = Environment.GetEnvironmentVariable("AWS_SQS_QUEUE_NAME");
        private static readonly bool IsQueueFifo = bool.Parse(Environment.GetEnvironmentVariable("AWS_SQS_IS_FIFO") ?? "false");
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        private readonly IAmazonSQS _sqsClient;

        public SqsWriter()
        {
            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(Region)
            };
            _sqsClient = new AmazonSQSClient(sqsConfig);
        }

        public async Task WriteLogEntryAsync(LogEntry logEntry)
        {
            var queueUrl = await _sqsClient.GetQueueUrlAsync(QueueName);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl.QueueUrl,
                MessageBody = JsonConvert.SerializeObject(logEntry),
                MessageAttributes = SqsMessageTypeAttribute.CreateAttributes(typeof(LogEntry).Name)
            };
            if (IsQueueFifo)
            {
                sendMessageRequest.MessageGroupId = typeof(LogEntry).Name;
                sendMessageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
            }

            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}