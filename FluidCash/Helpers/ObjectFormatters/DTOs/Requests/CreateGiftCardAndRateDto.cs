namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateGiftCardAndRateDto
(
    string? category,
    string? subCategory,
    string? countryCode,
    string? currency,
    decimal rate
);
