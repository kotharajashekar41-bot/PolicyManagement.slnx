using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.Interfaces;
using PolicyPlatform.Domain.Entities;

namespace PolicyPlatform.UnitTests.TestDoubles;

/// <summary>Hand-rolled test double instead of a mocking framework — keeps the test
/// project dependency-light and makes call-count assertions (used by the cache test)
/// trivial to read.</summary>
public class FakePolicyRepository : IPolicyRepository
{
    public List<Policy> Policies { get; } = [];
    public int GetSummaryCallCount { get; private set; }
    public int GetPagedCallCount { get; private set; }
    public List<Guid> LastFlaggedIds { get; private set; } = [];

    public Task<(IReadOnlyList<Policy> Items, int TotalCount)> GetPagedAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        GetPagedCallCount++;
        IReadOnlyList<Policy> items = Policies
            .Skip((parameters.Page - 1) * parameters.Size)
            .Take(parameters.Size)
            .ToList();
        return Task.FromResult((items, Policies.Count));
    }

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Policies.FirstOrDefault(p => p.Id == id));

    public Task<int> FlagForReviewAsync(IReadOnlyList<Guid> policyIds, CancellationToken cancellationToken)
    {
        LastFlaggedIds = policyIds.ToList();
        var matched = Policies.Where(p => policyIds.Contains(p.Id) && !p.FlaggedForReview).ToList();
        foreach (var policy in matched)
        {
            policy.FlaggedForReview = true;
        }

        return Task.FromResult(matched.Count);
    }

    public Task<PolicySummaryData> GetSummaryAsync(PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        GetSummaryCallCount++;
        var countsByStatus = Policies
            .GroupBy(p => p.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        var premiumByLob = Policies
            .GroupBy(p => p.LineOfBusiness.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.PremiumAmount));

        return Task.FromResult(new PolicySummaryData(countsByStatus, premiumByLob, 0));
    }
}
