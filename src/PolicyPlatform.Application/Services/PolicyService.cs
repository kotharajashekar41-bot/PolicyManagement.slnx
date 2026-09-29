using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.DTOs;
using PolicyPlatform.Application.Interfaces;

namespace PolicyPlatform.Application.Services;

public class PolicyService(
    IPolicyRepository repository,
    IMemoryCache cache,
    ILogger<PolicyService> logger) : IPolicyService
{
    private static readonly TimeSpan SummaryCacheDuration = TimeSpan.FromSeconds(30);

    public async Task<PagedResult<PolicyDto>> GetPagedAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(parameters, cancellationToken);
        return new PagedResult<PolicyDto>(
            items.Select(p => p.ToDto()).ToList(),
            parameters.Page,
            parameters.Size,
            totalCount);
    }

    public async Task<PolicyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var policy = await repository.GetByIdAsync(id, cancellationToken);
        return policy?.ToDto();
    }

    public async Task<FlagPoliciesResult> FlagForReviewAsync(
        FlagPoliciesRequest request, CancellationToken cancellationToken)
    {
        if (request.PolicyIds.Count == 0)
        {
            throw new AppValidationException("policyIds must contain at least one id.");
        }

        var flaggedCount = await repository.FlagForReviewAsync(request.PolicyIds, cancellationToken);
        logger.LogInformation(
            "Flagged {FlaggedCount} of {RequestedCount} requested policies for review",
            flaggedCount, request.PolicyIds.Count);

        return new FlagPoliciesResult(flaggedCount);
    }

    public async Task<PolicySummaryDto> GetSummaryAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        // Cache invalidation strategy: short absolute TTL rather than write-side
        // invalidation. Flagging-for-review doesn't change any summary figure
        // (status counts / premium totals / expiring-soon), and new policies
        // don't arrive via this API, so a 30s TTL bounds staleness without any
        // write-path needing to know about this cache's existence.
        var cacheKey = BuildSummaryCacheKey(parameters);

        if (cache.TryGetValue(cacheKey, out PolicySummaryDto? cached) && cached is not null)
        {
            return cached;
        }

        var data = await repository.GetSummaryAsync(parameters, cancellationToken);
        var dto = new PolicySummaryDto(data.CountsByStatus, data.PremiumByLineOfBusiness, data.ExpiringSoonCount);

        cache.Set(cacheKey, dto, SummaryCacheDuration);
        return dto;
    }

    private static string BuildSummaryCacheKey(PolicyQueryParameters p) =>
        $"summary:{p.Status}:{p.LineOfBusiness}:{p.Region}:{p.EffectiveDateFrom:O}:{p.EffectiveDateTo:O}:{p.Search}";
}
