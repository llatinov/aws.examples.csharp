using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;

namespace SqsWriter.Sqs
{
    public static class MessageAttributes
    {
        public const string MessageType = "MessageType";

        public static string GetMessageType(this Dictionary<string, MessageAttributeValue> attributes)
        {
            return attributes.SingleOrDefault(x => x.Key == MessageType).Value?.StringValue;
        }
    }
}