namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record GiftCardResponseDto
(
    string? category,
    string? subCategory,
    IEnumerable<GiftCardRateResponseDto> GiftCardRates
);