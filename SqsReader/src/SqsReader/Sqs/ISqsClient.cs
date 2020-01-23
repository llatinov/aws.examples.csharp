using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SqsReader.Sqs
{
    public interface ISqsClient
    {
        string GetQueueName();

        Task CreateQueueAsync();

        Task<SqsStatus> GetQueueStatusAsync();

        Task<List<Message>> GetMessagesAsync(CancellationToken cancellationToken = default);

        Task PostMessageAsync(string messageBody, string messageType);

        Task DeleteMessageAsync(string receiptHandle);

        Task RestoreFromDeadLetterQueueAsync(CancellationToken cancellationToken = default);
    }
}