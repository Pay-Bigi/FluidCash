namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateGiftCardDto
(
    string? category,
    string? subCategory,
    string? giftCardRateId
);
