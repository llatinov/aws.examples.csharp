using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Models;

namespace SqsReader.Dynamo
{
    public class MoviesRepository : IMoviesRepository
    {
        private const string TableName = "Movies";

        private readonly IDatabaseClient _client;
        private readonly IDynamoDBContext _context;
        private readonly DynamoDBOperationConfig _operationConfig;

        public MoviesRepository(IDatabaseClient client, IDynamoDBContext context)
        {
            _client = client;
            _context = context;
            _operationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = TableName
            };
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
                        AttributeName = "Title",
                        KeyType = "HASH"
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Title",
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

        public async Task SaveMovieAsync(Movie movie)
        {
            await _context.SaveAsync(movie, _operationConfig);
        }
    }
}