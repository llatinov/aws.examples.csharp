using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.TestUtilities;
using Models;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace DynamoDbLambdas.Tests
{
    public class ActorsFunctionTest
    {
        private readonly ITestOutputHelper _output;
        private readonly TestLambdaContext _context;
        private readonly Mock<ISqsWriter> _sqsWriterMock;
        private readonly Mock<IDynamoDbWriter> _dynamoDbWriterMock;

        public ActorsFunctionTest(ITestOutputHelper output)
        {
            _output = output;
            _context = new TestLambdaContext();
            _sqsWriterMock = new Mock<ISqsWriter>();
            _dynamoDbWriterMock = new Mock<IDynamoDbWriter>();
        }

        [Fact]
        public async Task TestFunction()
        {
            DynamoDBEvent evnt = new DynamoDBEvent
            {
                Records = new List<DynamoDBEvent.DynamodbStreamRecord>
                {
                    new DynamoDBEvent.DynamodbStreamRecord
                    {
                        AwsRegion = "us-west-2",
                        Dynamodb = JsonConvert.DeserializeObject<StreamRecord>(File.ReadAllText("actor.json"))
                    }
                }
            };

            _output.WriteLine(JsonConvert.SerializeObject(evnt, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyValuesResolver()
            }));


            var function = new ActorsFunction(_sqsWriterMock.Object, _dynamoDbWriterMock.Object);

            await function.FunctionHandler(evnt, _context);

            var testLogger = _context.Logger as TestLambdaLogger;
            Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
            _sqsWriterMock.Verify(x => x.WriteLogEntryAsync(It.Is<LogEntry>(y =>
                y.Message == "Actor 'TestFirstName TestLastName' processed by lambda")));
            _dynamoDbWriterMock.Verify(x => x.SerializeStreamRecord(It.IsAny<StreamRecord>()));
            _dynamoDbWriterMock.Verify(x => x.PutLogEntryAsync(It.IsAny<LogEntry>()));
        }
    }
}