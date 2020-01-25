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

        public ActorsFunction() : this(null, null) { }

        public ActorsFunction(ISqsWriter sqsWriter = null, IDynamoDbWriter dynamoDbWriter = null)
        {
            _sqsWriter = sqsWriter ?? new SqsWriter();
            _dynamoDbWriter = dynamoDbWriter ?? new DynamoDbWriter();
        }

        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                context.Logger.LogLine($"Event ID: {record.EventID}");
                context.Logger.LogLine($"Event Name: {record.EventName}");

                var streamRecordJson = _dynamoDbWriter.SerializeStreamRecord(record.Dynamodb);
                context.Logger.LogLine($"DynamoDB Record:{streamRecordJson}");
                context.Logger.LogLine(streamRecordJson);

                var actorName = $"{record.Dynamodb.NewImage["FirstName"].S} {record.Dynamodb.NewImage["LastName"].S}";
                var logEntry = new LogEntry
                {
                    Message = $"Actor '{actorName}' processed by lambda",
                    DateTime = DateTime.Now
                };
                await _sqsWriter.WriteLogEntryAsync(logEntry);
                await _dynamoDbWriter.PutLogEntryAsync(logEntry);
            }

            context.Logger.LogLine("Stream processing complete.");
        }
    }
}