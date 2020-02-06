using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbServerless
{
    public interface IDynamoDbReader
    {
        Task<QueryResponse> QueryAsync(QueryRequest queryRequest);
        string SerializeObject(object obj);
    }
}