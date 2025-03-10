namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record BuyGiftCardDto
(
    string? giftCardId,
    string? amount,
    string? walletId
);
