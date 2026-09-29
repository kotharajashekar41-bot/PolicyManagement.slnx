using Microsoft.EntityFrameworkCore;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.Interfaces;
using PolicyPlatform.Domain.Entities;
using PolicyPlatform.Infrastructure.Persistence;

namespace PolicyPlatform.Infrastructure.Repositories;

public class PolicyRepository(PolicyDbContext dbContext) : IPolicyRepository
{
    public async Task<(IReadOnlyList<Policy> Items, int TotalCount)> GetPagedAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(dbContext.Policies.AsNoTracking(), parameters);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, parameters.ParseSort());

        var items = await query
            .Skip((parameters.Page - 1) * parameters.Size)
            .Take(parameters.Size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<int> FlagForReviewAsync(IReadOnlyList<Guid> policyIds, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await dbContext.Policies
            .Where(p => policyIds.Contains(p.Id) && !p.FlaggedForReview)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.FlaggedForReview, true)
                    .SetProperty(p => p.UpdatedAt, now),
                cancellationToken);
    }

    public async Task<PolicySummaryData> GetSummaryAsync(
        PolicyQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(dbContext.Policies.AsNoTracking(), parameters);

        var statusCounts = await query
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var premiumByLob = await query
            .GroupBy(p => p.LineOfBusiness)
            .Select(g => new { LineOfBusiness = g.Key, Total = g.Sum(p => p.PremiumAmount) })
            .ToListAsync(cancellationToken);

        var thirtyDaysOut = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiringSoonCount = await query.CountAsync(
            p => p.Status == Domain.Enums.PolicyStatus.Active
                 && p.ExpiryDate >= today
                 && p.ExpiryDate <= thirtyDaysOut,
            cancellationToken);

        return new PolicySummaryData(
            statusCounts.ToDictionary(x => x.Status.ToString(), x => x.Count),
            premiumByLob.ToDictionary(x => x.LineOfBusiness.ToString(), x => x.Total),
            expiringSoonCount);
    }

    private static IQueryable<Policy> ApplyFilters(IQueryable<Policy> query, PolicyQueryParameters p)
    {
        if (p.Status.HasValue)
        {
            query = query.Where(x => x.Status == p.Status.Value);
        }

        if (p.LineOfBusiness.HasValue)
        {
            query = query.Where(x => x.LineOfBusiness == p.LineOfBusiness.Value);
        }

        if (!string.IsNullOrWhiteSpace(p.Region))
        {
            query = query.Where(x => x.Region == p.Region);
        }

        if (p.EffectiveDateFrom.HasValue)
        {
            query = query.Where(x => x.EffectiveDate >= p.EffectiveDateFrom.Value);
        }

        if (p.EffectiveDateTo.HasValue)
        {
            query = query.Where(x => x.EffectiveDate <= p.EffectiveDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.PolicyNumber, $"%{term}%") ||
                EF.Functions.Like(x.PolicyholderName, $"%{term}%") ||
                EF.Functions.Like(x.Underwriter, $"%{term}%"));
        }

        return query;
    }

    private static IQueryable<Policy> ApplySort(IQueryable<Policy> query, (string Field, bool Descending) sort) =>
        sort.Field.ToLowerInvariant() switch
        {
            "policynumber" => Order(query, x => x.PolicyNumber, sort.Descending),
            "policyholdername" => Order(query, x => x.PolicyholderName, sort.Descending),
            "lineofbusiness" => Order(query, x => x.LineOfBusiness, sort.Descending),
            "status" => Order(query, x => x.Status, sort.Descending),
            "premiumamount" => Order(query, x => x.PremiumAmount, sort.Descending),
            "effectivedate" => Order(query, x => x.EffectiveDate, sort.Descending),
            "expirydate" => Order(query, x => x.ExpiryDate, sort.Descending),
            "region" => Order(query, x => x.Region, sort.Descending),
            "underwriter" => Order(query, x => x.Underwriter, sort.Descending),
            "createdat" => Order(query, x => x.CreatedAt, sort.Descending),
            _ => throw new AppValidationException(
                $"Unknown sort field '{sort.Field}'. Valid fields: policyNumber, policyholderName, " +
                "lineOfBusiness, status, premiumAmount, effectiveDate, expiryDate, region, underwriter, createdAt.")
        };

    private static IQueryable<Policy> Order<TKey>(
        IQueryable<Policy> query, System.Linq.Expressions.Expression<Func<Policy, TKey>> keySelector, bool descending) =>
        descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
}
