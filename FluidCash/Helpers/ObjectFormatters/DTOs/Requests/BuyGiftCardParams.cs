namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record BuyGiftCardParams
(
    string? giftCardId,
    string? giftCardRateId,
    decimal amount,
    string? walletId,
    bool payFromWallet,
    bool isPaymentMade,
    string? paystackTransactionRef
);
