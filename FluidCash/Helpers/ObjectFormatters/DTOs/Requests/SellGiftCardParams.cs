namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record SellGiftCardParams
(
    DateTime? validUntil,
    string? otherDetails,
    IFormFileCollection? cardImages,
    decimal? cardAmount,
    string? giftCardId,
    string? giftCardRateId,
    string? walletId
);
