using Bogus;
using Microsoft.EntityFrameworkCore;
using PolicyPlatform.Domain.Entities;
using PolicyPlatform.Domain.Enums;

namespace PolicyPlatform.Infrastructure.Persistence.Seed;

public static class PolicySeeder
{
    private static readonly (string Region, string Currency)[] RegionCurrency =
    [
        ("Singapore", "SGD"), ("Hong Kong", "HKD"), ("Australia", "AUD"), ("Japan", "JPY"),
        ("Thailand", "THB"), ("Indonesia", "USD"), ("Malaysia", "USD"), ("Philippines", "USD")
    ];

    public static async Task SeedAsync(PolicyDbContext dbContext, int count = 220)
    {
        await dbContext.Database.MigrateAsync();

        if (await dbContext.Policies.AnyAsync())
        {
            return;
        }

        var policyNumberCounter = 100000;

        var faker = new Faker<Policy>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.PolicyNumber, _ => $"POL-{policyNumberCounter++}")
            .RuleFor(p => p.PolicyholderName, f => GenerateApacName(f))
            .RuleFor(p => p.LineOfBusiness, f => f.PickRandom<LineOfBusiness>())
            .RuleFor(p => p.Status, f => f.PickRandom<PolicyStatus>())
            .RuleFor(p => p.PremiumAmount, f => Math.Round(f.Random.Decimal(1_000m, 5_000_000m), 2))
            .RuleFor(p => p.Underwriter, f => f.Name.FullName())
            .RuleFor(p => p.FlaggedForReview, f => f.Random.Bool(0.12f));

        var policies = new List<Policy>(count);
        var now = DateTime.UtcNow;

        foreach (var _ in Enumerable.Range(0, count))
        {
            var policy = faker.Generate();
            var f = new Faker();

            var (region, currency) = f.PickRandom(RegionCurrency);
            policy.Region = region;
            policy.Currency = currency;

            // Spread effective dates across the past 2 years to the next 6 months so
            // status/expiry combinations (expired, expiring-soon, future-dated) occur naturally.
            var effectiveDate = f.Date.Between(now.AddYears(-2), now.AddMonths(6));
            var termMonths = f.PickRandom(6, 12, 12, 12, 24);
            policy.EffectiveDate = DateOnly.FromDateTime(effectiveDate);
            policy.ExpiryDate = DateOnly.FromDateTime(effectiveDate.AddMonths(termMonths));

            var createdAt = effectiveDate.AddDays(-f.Random.Int(1, 20));
            policy.CreatedAt = createdAt;
            policy.UpdatedAt = createdAt.AddDays(f.Random.Int(0, 30));

            policies.Add(policy);
        }

        await dbContext.Policies.AddRangeAsync(policies);
        await dbContext.SaveChangesAsync();
    }

    private static string GenerateApacName(Faker f)
    {
        var locale = f.PickRandom("en", "ja", "zh_CN", "id_ID");
        var localFaker = locale == "en" ? f : new Faker(locale);
        return localFaker.Name.FullName();
    }
}
