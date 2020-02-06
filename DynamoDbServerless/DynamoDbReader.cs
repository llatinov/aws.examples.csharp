using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbServerless
{
    public class DynamoDbReader : IDynamoDbReader
    {
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DynamoDbReader()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
            _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
        }

        public async Task<QueryResponse> QueryAsync(QueryRequest queryRequest)
        {
            return await _dynamoDbClient.QueryAsync(queryRequest);
        }
    }
}