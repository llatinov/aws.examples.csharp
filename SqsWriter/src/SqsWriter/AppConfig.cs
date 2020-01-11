using System;

namespace SqsWriter
{
    public class AppConfig
    {
        public AwsConfig AwsSettings { get; set; }

        public class AwsConfig
        {
            private const string FifoSuffix = ".fifo";
            private string _queueName;

            public string AwsRegion { get; set; }
            public string AwsAccessKey { get; set; }
            public string AwsSecretKey { get; set; }
            public bool AutomaticallyCreateQueue { get; set; }

            public string QueueName
            {
                get => IsFifo ? _queueName + FifoSuffix : _queueName;
                set => _queueName = value;
            }

            public string DeadLetterQueueName
            {
                get
                {
                    var deadLetter = _queueName + "-exceptions";
                    return IsFifo ? deadLetter + FifoSuffix : deadLetter;
                }
            }

            public bool IsFifo { get; set; }
            public int LongPollTimeSeconds { get; set; }

            public void UpdateFromEnvironment()
            {
                AwsRegion = Environment.GetEnvironmentVariable("AwsRegion") ?? AwsAccessKey;
                AwsAccessKey = Environment.GetEnvironmentVariable("AwsAccessKey") ?? AwsAccessKey;
                AwsSecretKey = Environment.GetEnvironmentVariable("AwsSecretKey") ?? AwsSecretKey;
            }
        }
    }
}