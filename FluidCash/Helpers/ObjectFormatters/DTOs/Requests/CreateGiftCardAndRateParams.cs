namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateGiftCardAndRateParams
(
    string? category,
    string? subCategory,
    string? countryCode,
    string? currency,
    decimal rate
);
