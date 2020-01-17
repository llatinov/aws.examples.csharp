using System.Threading.Tasks;
using SqsReader.Sqs;

namespace SqsReader.Services
{
    public interface ISqsConsumerService
    {
        Task<SqsStatus> GetStatusAsync();

        Task StartConsumingAsync();

        Task StopConsumingAsync();

        Task ReprocessMessagesAsync();
    }
}