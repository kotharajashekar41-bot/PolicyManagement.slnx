using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.DTOs;
using PolicyPlatform.Application.Services;
using PolicyPlatform.Domain.Entities;
using PolicyPlatform.Domain.Enums;
using PolicyPlatform.UnitTests.TestDoubles;
using Xunit;

namespace PolicyPlatform.UnitTests.Services;

public class PolicyServiceTests
{
    private static PolicyService CreateService(FakePolicyRepository repository) =>
        new(repository, new MemoryCache(new MemoryCacheOptions()), NullLogger<PolicyService>.Instance);

    private static Policy MakePolicy(PolicyStatus status = PolicyStatus.Active, bool flagged = false) => new()
    {
        Id = Guid.NewGuid(),
        PolicyNumber = $"POL-{Random.Shared.Next(100000, 999999)}",
        PolicyholderName = "Test",
        LineOfBusiness = LineOfBusiness.Property,
        Status = status,
        PremiumAmount = 1000m,
        Currency = "SGD",
        EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
        Region = "Singapore",
        Underwriter = "UW",
        FlaggedForReview = flagged,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task FlagForReviewAsync_WithEmptyIdList_ThrowsAppValidationException()
    {
        var service = CreateService(new FakePolicyRepository());

        var act = () => service.FlagForReviewAsync(new FlagPoliciesRequest([]), CancellationToken.None);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task FlagForReviewAsync_WithValidIds_ReturnsFlaggedCount()
    {
        var repository = new FakePolicyRepository();
        var policy = MakePolicy();
        repository.Policies.Add(policy);
        var service = CreateService(repository);

        var result = await service.FlagForReviewAsync(new FlagPoliciesRequest([policy.Id]), CancellationToken.None);

        result.FlaggedCount.Should().Be(1);
        repository.LastFlaggedIds.Should().ContainSingle().Which.Should().Be(policy.Id);
    }

    [Fact]
    public async Task GetSummaryAsync_CalledTwiceWithSameFilters_OnlyHitsRepositoryOnce()
    {
        var repository = new FakePolicyRepository();
        repository.Policies.Add(MakePolicy());
        var service = CreateService(repository);
        var parameters = new PolicyQueryParameters();

        await service.GetSummaryAsync(parameters, CancellationToken.None);
        await service.GetSummaryAsync(parameters, CancellationToken.None);

        repository.GetSummaryCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSummaryAsync_CalledWithDifferentFilters_HitsRepositoryForEachDistinctFilter()
    {
        var repository = new FakePolicyRepository();
        repository.Policies.Add(MakePolicy());
        var service = CreateService(repository);

        await service.GetSummaryAsync(new PolicyQueryParameters { Status = PolicyStatus.Active }, CancellationToken.None);
        await service.GetSummaryAsync(new PolicyQueryParameters { Status = PolicyStatus.Expired }, CancellationToken.None);

        repository.GetSummaryCallCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_MapsEntitiesToDtosAndPreservesPagingMetadata()
    {
        var repository = new FakePolicyRepository();
        repository.Policies.AddRange([MakePolicy(), MakePolicy(), MakePolicy()]);
        var service = CreateService(repository);

        var result = await service.GetPagedAsync(new PolicyQueryParameters { Page = 1, Size = 2 }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }
}
