using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace SqsReader.Dynamo
{
    public class DatabaseClient : IDatabaseClient
    {
        private const string UnknownStatus = "UNKNOWN";

        private readonly IAmazonDynamoDB _client;

        public DatabaseClient(IAmazonDynamoDB client)
        {
            _client = client;
        }

        public IAmazonDynamoDB GetClient()
        {
            return _client;
        }

        public async Task CreateTableAsync(CreateTableRequest createTableRequest)
        {
            var status = await GetTableStatusAsync(createTableRequest.TableName);
            if (status != UnknownStatus)
            {
                return;
            }

            await _client.CreateTableAsync(createTableRequest);

            await WaitUntilTableReady(createTableRequest.TableName);
        }

        public async Task<string> GetTableStatusAsync(string tableName)
        {
            try
            {
                var response = await _client.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });
                return response?.Table.TableStatus;
            }
            catch (ResourceNotFoundException)
            {
                return UnknownStatus;
            }
        }

        public async Task PutItemAsync(PutItemRequest putItemRequest)
        {
            await _client.PutItemAsync(putItemRequest);
        }

        private async Task WaitUntilTableReady(string tableName)
        {
            var status = await GetTableStatusAsync(tableName);
            for (var i = 0; i < 10 && status != "ACTIVE"; ++i)
            {
                await Task.Delay(500);
                status = await GetTableStatusAsync(tableName);
            }
        }
    }
}