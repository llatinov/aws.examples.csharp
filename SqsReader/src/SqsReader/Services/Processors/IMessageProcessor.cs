using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SqsReader.Services.Processors
{
    public interface IMessageProcessor
    {
        bool CanProcess(string messageType);

        Task ProcessAsync(Message message);
    }
}