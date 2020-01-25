using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
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
    public class MoviesFunctionTest
    {
        private readonly ITestOutputHelper _output;
        private readonly TestLambdaContext _context;
        private readonly Mock<ISqsWriter> _sqsWriterMock;
        private readonly Mock<IDynamoDbWriter> _dynamoDbWriterMock;

        public MoviesFunctionTest(ITestOutputHelper output)
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
                        Dynamodb = new StreamRecord
                        {
                            ApproximateCreationDateTime = DateTime.Now,
                            Keys = new Dictionary<string, AttributeValue>
                            {
                                {"Title", new AttributeValue {S = "Die Hard"}}
                            },
                            NewImage = new Dictionary<string, AttributeValue>
                            {
                                {"Title", new AttributeValue {S = "Title"}},
                                {"Genre", new AttributeValue {S = "0"}}
                            },
                            SequenceNumber = "25567400000000000393411126",
                            SizeBytes = 54,
                            StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
                        }
                    }
                }
            };

            _output.WriteLine(JsonConvert.SerializeObject(evnt, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyValuesResolver()
            }));


            var function = new MoviesFunction(_sqsWriterMock.Object, _dynamoDbWriterMock.Object);

            await function.FunctionHandler(evnt, _context);

            var testLogger = _context.Logger as TestLambdaLogger;
            Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
            _sqsWriterMock.Verify(x => x.WriteLogEntryAsync(It.Is<LogEntry>(y =>
                y.Message == $"Movie '{evnt.Records[0].Dynamodb.NewImage["Title"].S}' processed by lambda")));
            _dynamoDbWriterMock.Verify(x => x.SerializeStreamRecord(It.IsAny<StreamRecord>()));
            _dynamoDbWriterMock.Verify(x => x.PutLogEntryAsync(It.IsAny<LogEntry>()));
        }
    }
}