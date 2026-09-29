namespace PolicyPlatform.Api.Contracts;

/// <summary>Raw query-string shape. Kept string-typed for status/lineOfBusiness so we control
/// parsing (and can accept the "A&amp;H" wire value, which isn't a valid C# enum member name)
/// instead of letting ASP.NET Core's default enum model binder reject or mis-map it.</summary>
public class PolicyQueryRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
    public string? Sort { get; set; }
    public string? Status { get; set; }
    public string? LineOfBusiness { get; set; }
    public string? Region { get; set; }
    public DateOnly? EffectiveDateFrom { get; set; }
    public DateOnly? EffectiveDateTo { get; set; }
    public string? Search { get; set; }
}
