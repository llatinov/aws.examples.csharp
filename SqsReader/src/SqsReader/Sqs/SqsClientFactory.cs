using System;
using Amazon;
using Amazon.SQS;

namespace SqsReader.Sqs
{
    public class SqsClientFactory
    {
        public static AmazonSQSClient CreateClient(AppConfig appConfig)
        {
            var sqsConfig = new AmazonSQSConfig();
            Console.Write(appConfig.AwsSqsServiceUrl);
            if (!string.IsNullOrEmpty(appConfig.AwsSqsServiceUrl))
            {
                sqsConfig.ServiceURL = appConfig.AwsSqsServiceUrl;
            }
            else
            {
                sqsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(appConfig.AwsRegion);
            }
            var awsCredentials = new AwsCredentials(appConfig);
            return new AmazonSQSClient(awsCredentials, sqsConfig);
        }
    }
}