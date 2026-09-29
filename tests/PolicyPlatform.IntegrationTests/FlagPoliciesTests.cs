using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PolicyPlatform.Application.DTOs;
using Xunit;

namespace PolicyPlatform.IntegrationTests;

/// <summary>Kept out of PoliciesEndpointsTests: a successful flag mutates shared
/// fixture state, so it gets its own factory instance to stay order-independent.</summary>
public class FlagPoliciesTests : IAsyncLifetime
{
    private readonly PolicyApiFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await ((IAsyncLifetime)_factory).InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await ((IAsyncLifetime)_factory).DisposeAsync();
    }

    [Fact]
    public async Task Flag_WithValidIds_MarksPoliciesFlaggedAndIsIdempotent()
    {
        var request = new FlagPoliciesRequest([PolicyApiFactory.ActivePropertyId, PolicyApiFactory.PendingMarineId]);

        var firstResponse = await _client.PatchAsJsonAsync("/api/v1/policies/flag", request);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<FlagPoliciesResult>();

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResult!.FlaggedCount.Should().Be(2);

        var policy = await _client.GetFromJsonAsync<PolicyDto>($"/api/v1/policies/{PolicyApiFactory.ActivePropertyId}", TestJsonOptions.Default);
        policy!.FlaggedForReview.Should().BeTrue();

        // Re-flagging already-flagged policies matches zero rows — flag is idempotent.
        var secondResponse = await _client.PatchAsJsonAsync("/api/v1/policies/flag", request);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<FlagPoliciesResult>();

        secondResult!.FlaggedCount.Should().Be(0);
    }
}
