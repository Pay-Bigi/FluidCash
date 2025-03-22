namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetGiftCardFilterParams
(
    string? category,
    string? subCategory,    
    string? giftCardId,
    string? giftCardRateId
);
    