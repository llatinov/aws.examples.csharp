using System;

namespace SqsWriter.Sqs
{
    public class SqsStatus
    {
        public bool IsHealthy { get; set; }
        public bool? IsConsuming { get; set; }
        public string Region { get; set; }
        public string QueueName { get; set; }
        public int LongPollTimeSeconds { get; set; }
        public int ApproximateNumberOfMessages { get; set; }
        public int ApproximateNumberOfMessagesNotVisible { get; set; }
        public DateTime LastModifiedTimestamp { get; set; }
    }
}