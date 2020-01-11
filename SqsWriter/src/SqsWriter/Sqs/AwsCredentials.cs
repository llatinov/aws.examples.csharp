using Amazon.Runtime;

namespace SqsWriter.Sqs
{
    public class AwsCredentials : AWSCredentials
    {
        private readonly AppConfig.AwsConfig _awsConfig;

        public AwsCredentials(AppConfig.AwsConfig awsConfig)
        {
            _awsConfig = awsConfig;
        }

        public override ImmutableCredentials GetCredentials()
        {
            return new ImmutableCredentials(_awsConfig.AwsAccessKey, _awsConfig.AwsSecretKey, null);
        }
    }
}