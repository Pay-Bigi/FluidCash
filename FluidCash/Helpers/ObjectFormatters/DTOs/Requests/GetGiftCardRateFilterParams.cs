namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetGiftCardRateFilterParams
(
    string? countryCode,
    string? currency,
    decimal? rate,
    string? giftCardId,
    string? giftCardRateId
);
