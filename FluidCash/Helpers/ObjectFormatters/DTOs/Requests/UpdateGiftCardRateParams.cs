namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UpdateGiftCardRateParams
(
    string? giftCardRateId,
    string? countryCode,
    string? currency,
    decimal? rate,
    decimal? sellChargeRate
);
