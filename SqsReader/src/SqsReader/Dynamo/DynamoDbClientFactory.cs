using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace SqsReader.Dynamo
{
    public class DynamoDbClientFactory
    {
        public static AmazonDynamoDBClient CreateClient(AppConfig appConfig)
        {
            var dynamoDbConfig = new AmazonDynamoDBConfig();
            if (!string.IsNullOrEmpty(appConfig.LocalstackHostname))
            {
                // https://github.com/localstack/localstack/issues/1918
                dynamoDbConfig.ServiceURL = $"http://{appConfig.LocalstackHostname}:4569";
                var credentials = new BasicAWSCredentials("xxx", "xxx");
                return new AmazonDynamoDBClient(credentials, dynamoDbConfig);
            }

            dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion);
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonDynamoDBClient(awsCredentials, dynamoDbConfig);
        }
    }
}