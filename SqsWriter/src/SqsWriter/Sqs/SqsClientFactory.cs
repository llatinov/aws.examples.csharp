using Amazon;
using Amazon.Runtime;
using Amazon.SQS;

namespace SqsWriter.Sqs
{
    public class SqsClientFactory
    {
        public static AmazonSQSClient CreateClient(AppConfig appConfig)
        {
            var sqsConfig = new AmazonSQSConfig();
            if (!string.IsNullOrEmpty(appConfig.LocalstackHostname))
            {
                // https://github.com/localstack/localstack/issues/1918
                sqsConfig.ServiceURL = $"http://{appConfig.LocalstackHostname}:4576";
                var credentials = new BasicAWSCredentials("xxx", "xxx");
                return new AmazonSQSClient(credentials, sqsConfig);
            }

            sqsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion);
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonSQSClient(awsCredentials, sqsConfig);
        }
    }
}