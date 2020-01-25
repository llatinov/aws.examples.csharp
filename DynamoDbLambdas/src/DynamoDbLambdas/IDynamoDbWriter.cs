using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Models;

namespace DynamoDbLambdas
{
    public interface IDynamoDbWriter
    {
        Task PutLogEntryAsync(LogEntry logEntry);
        string SerializeStreamRecord(StreamRecord streamRecord);
    }
}