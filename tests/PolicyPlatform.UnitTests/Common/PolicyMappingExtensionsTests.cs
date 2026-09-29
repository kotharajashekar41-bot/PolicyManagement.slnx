using FluentAssertions;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Domain.Entities;
using PolicyPlatform.Domain.Enums;
using Xunit;

namespace PolicyPlatform.UnitTests.Common;

public class PolicyMappingExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllFields()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = createdAt.AddDays(3);

        var policy = new Policy
        {
            Id = id,
            PolicyNumber = "POL-100001",
            PolicyholderName = "Test Holder",
            LineOfBusiness = LineOfBusiness.AH,
            Status = PolicyStatus.Active,
            PremiumAmount = 12345.67m,
            Currency = "SGD",
            EffectiveDate = new DateOnly(2026, 1, 1),
            ExpiryDate = new DateOnly(2027, 1, 1),
            Region = "Singapore",
            Underwriter = "Jane Underwriter",
            FlaggedForReview = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        var dto = policy.ToDto();

        dto.Id.Should().Be(id);
        dto.PolicyNumber.Should().Be("POL-100001");
        dto.PolicyholderName.Should().Be("Test Holder");
        dto.LineOfBusiness.Should().Be(LineOfBusiness.AH);
        dto.Status.Should().Be(PolicyStatus.Active);
        dto.PremiumAmount.Should().Be(12345.67m);
        dto.Currency.Should().Be("SGD");
        dto.EffectiveDate.Should().Be(new DateOnly(2026, 1, 1));
        dto.ExpiryDate.Should().Be(new DateOnly(2027, 1, 1));
        dto.Region.Should().Be("Singapore");
        dto.Underwriter.Should().Be("Jane Underwriter");
        dto.FlaggedForReview.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
