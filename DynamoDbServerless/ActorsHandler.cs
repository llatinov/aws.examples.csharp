using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Models;

namespace DynamoDbServerless
{
    public class ActorsHandler
    {
        private readonly IDynamoDbReader _dynamoDbReader;

        public ActorsHandler() : this(null)
        {
        }

        public ActorsHandler(IDynamoDbReader dynamoDbReader = null)
        {
            _dynamoDbReader = dynamoDbReader ?? new DynamoDbReader();
        }

        public async Task<ActorsSearchResponse> QueryActors(ActorsSearchRequest request, ILambdaContext context)
        {
            var queryRequest = BuildQueryRequest(request.FirstName, request.LastName);
            var response = await _dynamoDbReader.QueryAsync(queryRequest);
            var results = BuildActorsResponse(response);
            var responseJson = _dynamoDbReader.SerializeObject(response);
            context.Logger.LogLine($"Query result: {responseJson}");

            return results;
        }

        private static QueryRequest BuildQueryRequest(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName))
            {
                throw new Exception("FirstName is mandatory");
            }

            var request = new QueryRequest("Actors")
            {
                KeyConditionExpression = "FirstName = :FirstName"
            };
            request.ExpressionAttributeValues.Add(":FirstName", new AttributeValue { S = firstName });

            if (!string.IsNullOrEmpty(lastName))
            {
                request.KeyConditionExpression += " AND #LastName = :LastName";
                request.ExpressionAttributeNames.Add("#LastName", "LastName");
                request.ExpressionAttributeValues.Add(":LastName", new AttributeValue { S = lastName });
            }

            return request;
        }

        private static ActorsSearchResponse BuildActorsResponse(QueryResponse response)
        {
            var actors = response.Items
                .Select(item => new Actor
                {
                    FirstName = item["FirstName"].S,
                    LastName = item["LastName"].S
                })
                .ToList();

            return new ActorsSearchResponse
            {
                Actors = actors,
                Count = actors.Count
            };
        }
    }
}