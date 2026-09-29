using PolicyPlatform.Domain.Enums;

namespace PolicyPlatform.Application.DTOs;

public record PolicyDto(
    Guid Id,
    string PolicyNumber,
    string PolicyholderName,
    LineOfBusiness LineOfBusiness,
    PolicyStatus Status,
    decimal PremiumAmount,
    string Currency,
    DateOnly EffectiveDate,
    DateOnly ExpiryDate,
    string Region,
    string Underwriter,
    bool FlaggedForReview,
    DateTime CreatedAt,
    DateTime UpdatedAt);
