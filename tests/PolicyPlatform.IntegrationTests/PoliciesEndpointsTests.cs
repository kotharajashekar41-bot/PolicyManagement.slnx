using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PolicyPlatform.Application.DTOs;
using Xunit;

namespace PolicyPlatform.IntegrationTests;

public class PoliciesEndpointsTests(PolicyApiFactory factory) : IClassFixture<PolicyApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_WithNoFilters_ReturnsAllSeededPolicies()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/v1/policies", TestJsonOptions.Default);

        result!.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task List_FilteredByStatus_ReturnsOnlyMatchingPolicies()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/v1/policies?status=Active", TestJsonOptions.Default);

        result!.TotalCount.Should().Be(1);
        result.Items.Single().Id.Should().Be(PolicyApiFactory.ActivePropertyId);
    }

    [Fact]
    public async Task List_FilteredByLineOfBusinessAH_ReturnsNoneForThisDataset()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/v1/policies?lineOfBusiness=A%26H", TestJsonOptions.Default);

        result!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task List_WithSearchMatchingPolicyholderName_ReturnsMatch()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/v1/policies?search=Chandra", TestJsonOptions.Default);

        result!.Items.Single().Id.Should().Be(PolicyApiFactory.PendingMarineId);
    }

    [Fact]
    public async Task List_SortedByPremiumDescending_ReturnsHighestPremiumFirst()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PolicyDto>>("/api/v1/policies?sort=premiumAmount,desc", TestJsonOptions.Default);

        result!.Items.First().Id.Should().Be(PolicyApiFactory.PendingMarineId); // 50,000 premium
    }

    [Fact]
    public async Task List_WithUnknownSortField_Returns400ProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/policies?sort=notAField,asc");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsPolicy()
    {
        var policy = await _client.GetFromJsonAsync<PolicyDto>($"/api/v1/policies/{PolicyApiFactory.ExpiredCasualtyId}", TestJsonOptions.Default);

        policy!.PolicyNumber.Should().Be("POL-000002");
    }

    [Fact]
    public async Task GetById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/policies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Summary_ReturnsCountsAndExpiringSoon()
    {
        var summary = await _client.GetFromJsonAsync<PolicySummaryDto>("/api/v1/policies/summary");

        summary!.CountsByStatus["Active"].Should().Be(1);
        summary.CountsByStatus["Expired"].Should().Be(1);
        summary.CountsByStatus["Pending"].Should().Be(1);
        summary.ExpiringSoonCount.Should().Be(1); // the Active property expiring in 10 days
    }

    [Fact]
    public async Task Flag_WithEmptyIdArray_Returns400()
    {
        var response = await _client.PatchAsJsonAsync("/api/v1/policies/flag", new FlagPoliciesRequest([]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
