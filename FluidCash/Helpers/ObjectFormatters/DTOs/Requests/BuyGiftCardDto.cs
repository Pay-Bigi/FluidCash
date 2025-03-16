namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record BuyGiftCardDto
(
    string? giftCardId,
    string? giftCardRateId,
    decimal amount,
    string? walletId,
    bool payFromWallet,
    bool isPaymentMade,
    string? paystackTransactionRef
);
