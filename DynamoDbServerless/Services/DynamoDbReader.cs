using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbServerless.Services
{
    public class DynamoDbReader : IDynamoDbReader
    {
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DynamoDbReader()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(Region)
            };
            _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
        }

        public async Task<QueryResponse> QueryAsync(QueryRequest queryRequest)
        {
            return await _dynamoDbClient.QueryAsync(queryRequest);
        }

        public async Task<Document> GetDocumentAsync(string tableName, string documentKey)
        {
            var table = Table.LoadTable(_dynamoDbClient, new TableConfig(tableName));
            return await table.GetItemAsync(new Primitive(documentKey));
        }
    }
}