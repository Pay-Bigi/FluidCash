namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record UpdateGiftCardDto
(
    string? giftCardId,
    string? Category,
    string? SubCategory,
    string? giftCardRateId
);
