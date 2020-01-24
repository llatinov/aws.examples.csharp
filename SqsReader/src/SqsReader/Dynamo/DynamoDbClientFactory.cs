using Amazon;
using Amazon.DynamoDBv2;

namespace SqsReader.Dynamo
{
    public class DynamoDbClientFactory
    {
        public static AmazonDynamoDBClient CreateClient(AppConfig appConfig)
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            if (!string.IsNullOrEmpty(appConfig.AwsDynamoServiceUrl))
            {
                dynamoDbConfig.ServiceURL = appConfig.AwsDynamoServiceUrl;
            }
            else
            {
                dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion);
            }
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonDynamoDBClient(awsCredentials, dynamoDbConfig);
        }
    }
}