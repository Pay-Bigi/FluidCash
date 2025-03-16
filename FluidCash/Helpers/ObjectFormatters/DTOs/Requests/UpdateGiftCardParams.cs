namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UpdateGiftCardParams
(
    string? giftCardId,
    string? category,
    string? subCategory,
    string? giftCardRateId
);
