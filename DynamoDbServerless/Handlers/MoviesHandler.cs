using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DynamoDbServerless.Services;
using Models;

namespace DynamoDbServerless.Handlers
{
    public class MoviesHandler
    {
        private const string TableName = "Movies";

        private readonly IDynamoDbReader _dynamoDbReader;
        private readonly IJsonConverter _jsonConverter;

        public MoviesHandler() : this(null, null)
        {
        }

        public MoviesHandler(IDynamoDbReader dynamoDbReader, IJsonConverter jsonConverter)
        {
            _dynamoDbReader = dynamoDbReader ?? new DynamoDbReader();
            _jsonConverter = jsonConverter ?? new JsonConverter();
        }

        public async Task<APIGatewayProxyResponse> GetMovie(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"Query request: {_jsonConverter.SerializeObject(request)}");

            var title = WebUtility.UrlDecode(request.PathParameters["title"]);
            var document = await _dynamoDbReader.GetDocumentAsync(TableName, title);
            context.Logger.LogLine($"Query response: {_jsonConverter.SerializeObject(document)}");

            if (document == null)
            {
                return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.NotFound };
            }

            var movie = new Movie
            {
                Title = document["Title"],
                Genre = (MovieGenre)int.Parse(document["Genre"])
            };
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = _jsonConverter.SerializeObject(movie)
            };
        }
    }
}