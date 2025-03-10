using FluidCash.Helpers.Enums;

namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record WalletTradingResponse
(
    string? tradeId,
    string? exchangeValue,
    string? cardImageUrl,
    decimal cardAmount,
    decimal exchangeRate,
    DateTime? tradeDateTime,
    TradeType? tradeType,
    DateTime? validUntil,
    string? otherDetails,
    string? giftCardId,
    string? walletId
    //Consider adding card and wallet details
);