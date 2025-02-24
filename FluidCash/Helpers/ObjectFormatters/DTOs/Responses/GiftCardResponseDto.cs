namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record GiftCardResponseDto
(
    string? category,
    string? subCategory,
    string? countryCode,
    string? currency,
    decimal rate,
    string? giftCardId,
    string? giftCardRateId
);