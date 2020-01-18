using System.Threading.Tasks;
using SqsReader.Sqs;

namespace SqsReader.Services
{
    public interface ISqsConsumerService
    {
        Task<SqsStatus> GetStatusAsync();

        void StartConsuming();

        void StopConsuming();

        Task ReprocessMessagesAsync();
    }
}