using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Models;

namespace SqsReader.Dynamo
{
    public class ActorsRepository : IActorsRepository
    {
        private const string TableName = "Actors";

        private readonly IDatabaseClient _client;

        public ActorsRepository(IDatabaseClient client)
        {
            _client = client;
        }

        public async Task CreateTableAsync()
        {
            var request = new CreateTableRequest
            {
                TableName = TableName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "FirstName",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "LastName",
                        KeyType = "RANGE"
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                   new AttributeDefinition
                    {
                        AttributeName = "FirstName",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "LastName",
                        AttributeType = "S"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                },
                StreamSpecification = new StreamSpecification
                {
                    StreamEnabled = true,
                    StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
                }
            };

            await _client.CreateTableAsync(request);
        }

        public async Task SaveActorAsync(Actor actor)
        {
            var request = new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {"FirstName", new AttributeValue {S = actor.FirstName}},
                    {"LastName", new AttributeValue {S = actor.LastName}}
                }
            };
            await _client.PutItemAsync(request);
        }
    }
}