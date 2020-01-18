using System.Runtime.Serialization;

namespace Models
{
    public enum MovieGenre
    {
        [EnumMember(Value = "Action Movie")]
        Action,
        [EnumMember(Value = "Drama Movie")]
        Drama
    }
}