namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetGiftCardDto
(
    string? category,
    string? subCategory,    
    string? giftCardId
);
    