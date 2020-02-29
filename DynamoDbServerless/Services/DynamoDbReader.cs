using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace DynamoDbServerless.Services
{
    public class DynamoDbReader : IDynamoDbReader
    {
        private static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
        private static readonly string LocalstackHostname = Environment.GetEnvironmentVariable("LOCALSTACK_HOSTNAME");

        private readonly IAmazonDynamoDB _dynamoDbClient;

        public DynamoDbReader()
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            if (!string.IsNullOrEmpty(LocalstackHostname))
            {
                // https://github.com/localstack/localstack/issues/1918
                dynamoDbConfig.ServiceURL = $"http://{LocalstackHostname}:4569";
                var credentials = new BasicAWSCredentials("xxx", "xxx");
                _dynamoDbClient = new AmazonDynamoDBClient(credentials, dynamoDbConfig);
            }
            else
            {
                dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
                _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
            }
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