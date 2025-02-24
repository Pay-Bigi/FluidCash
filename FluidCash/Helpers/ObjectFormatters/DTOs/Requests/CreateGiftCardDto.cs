namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateGiftCardDto
(
    string? Category,
    string? SubCategory,
    string? giftCardRateId
);
