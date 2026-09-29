using PolicyPlatform.Application.Common;
using PolicyPlatform.Domain.Entities;

namespace PolicyPlatform.Application.Interfaces;

public record PolicySummaryData(
    IReadOnlyDictionary<string, int> CountsByStatus,
    IReadOnlyDictionary<string, decimal> PremiumByLineOfBusiness,
    int ExpiringSoonCount);

public interface IPolicyRepository
{
    Task<(IReadOnlyList<Policy> Items, int TotalCount)> GetPagedAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken);

    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<int> FlagForReviewAsync(IReadOnlyList<Guid> policyIds, CancellationToken cancellationToken);

    Task<PolicySummaryData> GetSummaryAsync(PolicyQueryParameters parameters, CancellationToken cancellationToken);
}
