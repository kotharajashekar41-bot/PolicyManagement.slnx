using Microsoft.AspNetCore.Mvc;
using PolicyPlatform.Api.Contracts;
using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.DTOs;
using PolicyPlatform.Application.Interfaces;

namespace PolicyPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/policies")]
public class PoliciesController(IPolicyService policyService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<PolicyDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PolicyDto>>> List(
        [FromQuery] PolicyQueryRequest request, CancellationToken cancellationToken)
    {
        var parameters = ToQueryParameters(request);
        var result = await policyService.GetPagedAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType<PolicySummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PolicySummaryDto>> Summary(
        [FromQuery] PolicyQueryRequest request, CancellationToken cancellationToken)
    {
        var parameters = ToQueryParameters(request);
        var result = await policyService.GetSummaryAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var policy = await policyService.GetByIdAsync(id, cancellationToken);
        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpPatch("flag")]
    [ProducesResponseType<FlagPoliciesResult>(StatusCodes.Status200OK)]
    public async Task<ActionResult<FlagPoliciesResult>> Flag(
        [FromBody] FlagPoliciesRequest request, CancellationToken cancellationToken)
    {
        var result = await policyService.FlagForReviewAsync(request, cancellationToken);
        return Ok(result);
    }

    private static PolicyQueryParameters ToQueryParameters(PolicyQueryRequest request) => new()
    {
        Page = request.Page,
        Size = request.Size,
        Sort = request.Sort,
        Status = EnumParsing.ParseStatus(request.Status),
        LineOfBusiness = EnumParsing.ParseLineOfBusiness(request.LineOfBusiness),
        Region = request.Region,
        EffectiveDateFrom = request.EffectiveDateFrom,
        EffectiveDateTo = request.EffectiveDateTo,
        Search = request.Search
    };
}
