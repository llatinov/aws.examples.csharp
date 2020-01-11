using System.Runtime.Serialization;

namespace SqsReader.Sqs.Models
{
    public enum MovieGenre
    {
        [EnumMember(Value = "Action Movie")]
        Action,
        [EnumMember(Value = "Drama Movie")]
        Drama
    }
}