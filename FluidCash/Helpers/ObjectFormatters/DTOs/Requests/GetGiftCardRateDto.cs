namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetGiftCardRateDto
(
    string? countryCode,
    string? currency,
    decimal? rate,
    string? giftCardId,
    string? giftCardRateId
);
