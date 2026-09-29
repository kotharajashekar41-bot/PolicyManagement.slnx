namespace PolicyPlatform.Application.DTOs;

public record PolicySummaryDto(
    IReadOnlyDictionary<string, int> CountsByStatus,
    IReadOnlyDictionary<string, decimal> PremiumByLineOfBusiness,
    int ExpiringSoonCount);

public record FlagPoliciesRequest(IReadOnlyList<Guid> PolicyIds);

public record FlagPoliciesResult(int FlaggedCount);
