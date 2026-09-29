using PolicyPlatform.Application.Common;
using PolicyPlatform.Domain.Enums;

namespace PolicyPlatform.Api.Contracts;

public static class EnumParsing
{
    public static PolicyStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<PolicyStatus>(value, ignoreCase: true, out var status))
        {
            return status;
        }

        throw new AppValidationException(
            $"Invalid status '{value}'. Valid values: Active, Expired, Pending, Cancelled.");
    }

    public static LineOfBusiness? ParseLineOfBusiness(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Equals("A&H", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("AH", StringComparison.OrdinalIgnoreCase))
        {
            return LineOfBusiness.AH;
        }

        if (Enum.TryParse<LineOfBusiness>(normalized, ignoreCase: true, out var lob))
        {
            return lob;
        }

        throw new AppValidationException(
            $"Invalid lineOfBusiness '{value}'. Valid values: Property, Casualty, A&H, Marine.");
    }
}
