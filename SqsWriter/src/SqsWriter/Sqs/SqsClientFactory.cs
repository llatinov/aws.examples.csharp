using Amazon;
using Amazon.SQS;

namespace SqsWriter.Sqs
{
    public class SqsClientFactory
    {
        public static AmazonSQSClient CreateClient(AppConfig appConfig)
        {
            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion)
            };
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonSQSClient(awsCredentials, sqsConfig);
        }
    }
}