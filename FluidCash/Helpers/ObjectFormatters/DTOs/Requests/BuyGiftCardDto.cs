namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record BuyGiftCardDto
(
    string? giftCardId,
    decimal amount,
    string? walletId
);
