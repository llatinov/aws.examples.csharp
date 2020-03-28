using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Models;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace DynamoDbLambdas
{
    public class DynamoDbWriter : IDynamoDbWriter
    {
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly JsonSerializer _jsonSerializer;

        public DynamoDbWriter()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(Region)
            };
            _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
            _jsonSerializer = new JsonSerializer();
        }

        public async Task PutLogEntryAsync(LogEntry logEntry)
        {
            var dateTime = logEntry.DateTime.ToString(CultureInfo.InvariantCulture);
            var request = new PutItemRequest
            {
                TableName = "LogEntries",
                Item = new Dictionary<string, AttributeValue>
                {
                    {"Message", new AttributeValue {S = logEntry.Message}},
                    {"DateTime", new AttributeValue {S = dateTime}}
                }
            };

            await _dynamoDbClient.PutItemAsync(request);
        }

        public string SerializeStreamRecord(StreamRecord streamRecord)
        {
            using (var writer = new StringWriter())
            {
                _jsonSerializer.Serialize(writer, streamRecord);
                return writer.ToString();
            }
        }
    }
}