using System.Text.Json.Serialization;

namespace PolicyPlatform.Domain.Enums;

public enum LineOfBusiness
{
    Property,
    Casualty,

    [JsonStringEnumMemberName("A&H")]
    AH,

    Marine
}
