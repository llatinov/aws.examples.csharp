using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace SqsReader.Dynamo
{
    public interface IDatabaseClient
    {
        Task CreateTableAsync(CreateTableRequest createTableRequest);

        Task PutItemAsync(PutItemRequest putItemRequest);
    }
}