namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record GiftCardRateResponseDto
(
    string? countryCode,
    string? currency,
    decimal rate,
    string? giftCardId,
    string? giftCardRateId,
    decimal? sellChargeRate
);
