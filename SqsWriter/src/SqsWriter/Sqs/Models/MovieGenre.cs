using System.Runtime.Serialization;

namespace SqsWriter.Sqs.Models
{
    public enum MovieGenre
    {
        [EnumMember(Value = "Action Movie")]
        Action,
        [EnumMember(Value = "Drama Movie")]
        Drama
    }
}