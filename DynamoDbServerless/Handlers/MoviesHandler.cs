using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DynamoDbServerless.Services;
using Models;

namespace DynamoDbServerless.Handlers
{
    public class MoviesHandler
    {
        private const string TitleKey = "Title";

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

        public async Task<APIGatewayProxyResponse> GetMoviesByGenre(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"Query request: {_jsonConverter.SerializeObject(request)}");

            var title = WebUtility.UrlDecode(request.PathParameters["title"]);
            var getItemRequest = BuildGetItemRequest(title);
            var getItemResponse = await _dynamoDbReader.GetItemAsync(getItemRequest);
            context.Logger.LogLine($"Query response: {_jsonConverter.SerializeObject(getItemResponse)}");

            if (!getItemResponse.Item.ContainsKey(TitleKey))
            {
                return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.NotFound };
            }

            var movie = new Movie
            {
                Title = getItemResponse.Item[TitleKey].S,
                Genre = (MovieGenre)int.Parse(getItemResponse.Item["Genre"].N)
            };
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = _jsonConverter.SerializeObject(movie)
            };
        }

        private static GetItemRequest BuildGetItemRequest(string title)
        {
            return new GetItemRequest
            {
                TableName = "Movies",
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Title", new AttributeValue {S = title}}
                }
            };
        }
    }
}