namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateGiftCardParams
(
    string? category,
    string? subCategory,
    string? giftCardRateId
);
