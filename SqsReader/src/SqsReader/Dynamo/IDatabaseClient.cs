using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace SqsReader.Dynamo
{
    public interface IDatabaseClient
    {
        IAmazonDynamoDB GetClient();

        Task CreateTableAsync(CreateTableRequest createTableRequest);

        Task<string> GetTableStatusAsync(string tableName);

        Task PutItemAsync(PutItemRequest putItemRequest);
    }
}