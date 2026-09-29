using PolicyPlatform.Domain.Enums;

namespace PolicyPlatform.Application.Common;

public class PolicyQueryParameters
{
    private int _page = 1;
    private int _size = 20;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int Size
    {
        get => _size;
        init => _size = value switch
        {
            < 1 => 1,
            > 200 => 200,
            _ => value
        };
    }

    /// <summary>Format: "field,direction" e.g. "premiumAmount,desc". Defaults to "createdAt,desc".</summary>
    public string? Sort { get; init; }

    public PolicyStatus? Status { get; init; }
    public LineOfBusiness? LineOfBusiness { get; init; }
    public string? Region { get; init; }
    public DateOnly? EffectiveDateFrom { get; init; }
    public DateOnly? EffectiveDateTo { get; init; }
    public string? Search { get; init; }

    public (string Field, bool Descending) ParseSort()
    {
        if (string.IsNullOrWhiteSpace(Sort))
        {
            return ("createdAt", true);
        }

        var parts = Sort.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var field = parts.Length > 0 ? parts[0] : "createdAt";
        var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (field, descending);
    }
}
