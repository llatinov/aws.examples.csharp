using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbServerless.Services
{
    public interface IDynamoDbReader
    {
        Task<QueryResponse> QueryAsync(QueryRequest queryRequest);
    }
}