using System.Threading.Tasks;
using SqsReader.Sqs;

namespace SqsReader.Services
{
    public interface ISqsConsumerService
    {
        Task<SqsStatus> GetStatus();

        void StartConsuming();

        void StopConsuming();

        Task ReprocessMessages();
    }
}