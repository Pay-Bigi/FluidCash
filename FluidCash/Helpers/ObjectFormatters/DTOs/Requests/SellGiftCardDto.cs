namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record SellGiftCardDto
(
    DateTime? validUntil,
    string? otherDetails,
    IFormFile? cardImage,
    decimal? cardAmount,
    string? giftCardId,
    string? giftCardRateId,
    string? walletId
);
