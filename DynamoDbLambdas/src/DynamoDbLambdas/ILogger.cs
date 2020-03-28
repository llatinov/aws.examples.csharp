namespace DynamoDbLambdas
{
    public interface ILogger
    {
        void LogInformation(string messageTemplate, params object[] arguments);
    }
}