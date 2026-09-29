using PolicyPlatform.Application.DTOs;
using PolicyPlatform.Domain.Entities;

namespace PolicyPlatform.Application.Common;

public static class PolicyMappingExtensions
{
    public static PolicyDto ToDto(this Policy policy) => new(
        policy.Id,
        policy.PolicyNumber,
        policy.PolicyholderName,
        policy.LineOfBusiness,
        policy.Status,
        policy.PremiumAmount,
        policy.Currency,
        policy.EffectiveDate,
        policy.ExpiryDate,
        policy.Region,
        policy.Underwriter,
        policy.FlaggedForReview,
        policy.CreatedAt,
        policy.UpdatedAt);
}
