namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UpdateGiftCardRateDto
(
    string? giftCardRateId,
    string? countryCode,
    string? currency,
    decimal rate
);
