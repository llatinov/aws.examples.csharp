using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace DynamoDbServerless
{
    public class DynamoDbReader : IDynamoDbReader
    {
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly JsonSerializer _jsonSerializer;

        public DynamoDbReader()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
            _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
            _jsonSerializer = new JsonSerializer();
        }

        public async Task<QueryResponse> QueryAsync(QueryRequest queryRequest)
        {
            return await _dynamoDbClient.QueryAsync(queryRequest);
        }

        public string SerializeObject(object obj)
        {
            using (var writer = new StringWriter())
            {
                _jsonSerializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }
    }
}