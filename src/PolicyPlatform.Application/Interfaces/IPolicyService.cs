using PolicyPlatform.Application.Common;
using PolicyPlatform.Application.DTOs;

namespace PolicyPlatform.Application.Interfaces;

public interface IPolicyService
{
    Task<PagedResult<PolicyDto>> GetPagedAsync(PolicyQueryParameters parameters, CancellationToken cancellationToken);

    Task<PolicyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FlagPoliciesResult> FlagForReviewAsync(FlagPoliciesRequest request, CancellationToken cancellationToken);

    Task<PolicySummaryDto> GetSummaryAsync(PolicyQueryParameters parameters, CancellationToken cancellationToken);
}
