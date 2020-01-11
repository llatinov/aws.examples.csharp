using Amazon.SQS.Model;

namespace SqsReader.Services.Processors
{
    public interface IMessageProcessor
    {
        bool CanProcess(string messageType);

        void Process(Message message);
    }
}