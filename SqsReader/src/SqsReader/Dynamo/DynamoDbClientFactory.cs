using Amazon;
using Amazon.DynamoDBv2;

namespace SqsReader.Dynamo
{
    public class DynamoDbClientFactory
    {
        public static AmazonDynamoDBClient CreateClient(AppConfig appConfig)
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion)
            };
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonDynamoDBClient(awsCredentials, dynamoDbConfig);
        }
    }
}