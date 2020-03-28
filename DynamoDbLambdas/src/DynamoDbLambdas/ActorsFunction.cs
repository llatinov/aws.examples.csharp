using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Models;

namespace DynamoDbLambdas
{
    public class ActorsFunction
    {
        private readonly ISqsWriter _sqsWriter;
        private readonly IDynamoDbWriter _dynamoDbWriter;
        private readonly ILogger _logger;

        public ActorsFunction() : this(null, null, null) { }

        public ActorsFunction(ISqsWriter sqsWriter, IDynamoDbWriter dynamoDbWriter, ILogger logger)
        {
            _sqsWriter = sqsWriter ?? new SqsWriter();
            _dynamoDbWriter = dynamoDbWriter ?? new DynamoDbWriter();
            _logger = logger ?? new Logger();
        }

        public async Task ActorsFunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                context.Logger.LogLine($"Event ID: {record.EventID}");
                context.Logger.LogLine($"Event Name: {record.EventName}");

                var streamRecordJson = _dynamoDbWriter.SerializeStreamRecord(record.Dynamodb);
                context.Logger.LogLine($"DynamoDB Record:{streamRecordJson}");
                context.Logger.LogLine(streamRecordJson);

                var firstName = record.Dynamodb.NewImage["FirstName"].S;
                var lastName = record.Dynamodb.NewImage["LastName"].S;
                var logEntry = new LogEntry
                {
                    Message = $"Actor '{firstName} {lastName}' processed by lambda",
                    DateTime = DateTime.Now
                };
                _logger.LogInformation("ActorsFunctionHandler invoked with {FirstName} and {LastName}",
                    firstName, lastName);

                await _sqsWriter.WriteLogEntryAsync(logEntry);
                await _dynamoDbWriter.PutLogEntryAsync(logEntry);
            }

            context.Logger.LogLine("Stream processing complete.");
        }
    }
}