using FluentAssertions;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Domain.Enums;
using Xunit;

namespace PolicyPlatform.UnitTests.Common;

public class PolicyQueryParametersTests
{
    [Fact]
    public void ParseSort_WithNoSortProvided_DefaultsToCreatedAtDescending()
    {
        var parameters = new PolicyQueryParameters();

        var (field, descending) = parameters.ParseSort();

        field.Should().Be("createdAt");
        descending.Should().BeTrue();
    }

    [Theory]
    [InlineData("premiumAmount,desc", "premiumAmount", true)]
    [InlineData("premiumAmount,asc", "premiumAmount", false)]
    [InlineData("premiumAmount", "premiumAmount", false)]
    [InlineData(" policyNumber , DESC ", "policyNumber", true)]
    public void ParseSort_ParsesFieldAndDirection(string sort, string expectedField, bool expectedDescending)
    {
        var parameters = new PolicyQueryParameters { Sort = sort };

        var (field, descending) = parameters.ParseSort();

        field.Should().Be(expectedField);
        descending.Should().Be(expectedDescending);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(7, 7)]
    public void Page_IsClampedToAtLeastOne(int requested, int expected)
    {
        var parameters = new PolicyQueryParameters { Page = requested };

        parameters.Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    [InlineData(50, 50)]
    public void Size_IsClampedBetweenOneAndTwoHundred(int requested, int expected)
    {
        var parameters = new PolicyQueryParameters { Size = requested };

        parameters.Size.Should().Be(expected);
    }

    [Fact]
    public void Filters_DefaultToNull()
    {
        var parameters = new PolicyQueryParameters();

        parameters.Status.Should().BeNull();
        parameters.LineOfBusiness.Should().BeNull();
        parameters.Region.Should().BeNull();
        parameters.Search.Should().BeNull();
    }

    [Fact]
    public void Status_CanBeSet()
    {
        var parameters = new PolicyQueryParameters { Status = PolicyStatus.Active };

        parameters.Status.Should().Be(PolicyStatus.Active);
    }
}
