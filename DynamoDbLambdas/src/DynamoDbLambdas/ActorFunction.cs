using System.IO;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DynamoDbLambdas
{
    public class ActorFunction
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                context.Logger.LogLine($"Event ID: {record.EventID}");
                context.Logger.LogLine($"Event Name: {record.EventName}");

                string streamRecordJson = SerializeStreamRecord(record.Dynamodb);
                context.Logger.LogLine($"DynamoDB Record:");
                context.Logger.LogLine(streamRecordJson );
            }

            context.Logger.LogLine("Stream processing complete.");
        }

        private string SerializeStreamRecord(StreamRecord streamRecord)
        {
            using (var writer = new StringWriter())
            {
                JsonSerializer.Serialize(writer, streamRecord);
                return writer.ToString();
            }
        }
    }
}