namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UpdateGiftCardDto
(
    string? giftCardId,
    string? category,
    string? subCategory,
    string? giftCardRateId
);
