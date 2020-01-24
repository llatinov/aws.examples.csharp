using System.Collections.Generic;
using System.IO;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace DynamoDbLambdas.Tests
{
    public class ActorFunctionTest
    {
        private readonly ITestOutputHelper output;

        public ActorFunctionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestFunction()
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

            output.WriteLine(JsonConvert.SerializeObject(evnt, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyValuesResolver()
            }));

            var context = new TestLambdaContext();
            var function = new ActorFunction();

            function.FunctionHandler(evnt, context);

            var testLogger = context.Logger as TestLambdaLogger;
            Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }
    }
}