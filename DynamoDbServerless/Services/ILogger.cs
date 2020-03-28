namespace DynamoDbServerless.Services
{
    public interface ILogger
    {
        void LogInformation(string messageTemplate, params object[] arguments);
    }
}