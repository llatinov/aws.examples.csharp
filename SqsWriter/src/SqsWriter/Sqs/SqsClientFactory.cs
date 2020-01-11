using Amazon;
using Amazon.SQS;

namespace SqsWriter.Sqs
{
    public class SqsClientFactory
    {
        public static AmazonSQSClient CreateClient(AppConfig.AwsConfig awsConfig)
        {
            var sqsConfig = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.AwsRegion)
            };
            var awsCredentials = new AwsCredentials(awsConfig);
            return new AmazonSQSClient(awsCredentials, sqsConfig);
        }
    }
}