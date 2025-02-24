namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetGiftCardDto
(
    string? category,
    string? subCategory,
    string? countryCode,
    string? currency,
    decimal rate,
    string? giftCardId,
    string? giftCardRateId
);
    