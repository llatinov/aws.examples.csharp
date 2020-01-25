using System.Threading.Tasks;
using Models;

namespace DynamoDbLambdas
{
    public interface ISqsWriter
    {
        Task WriteLogEntryAsync(LogEntry logEntry);
    }
}