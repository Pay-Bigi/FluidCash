using FluidCash.Helpers.Enums;

namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record WalletTradingResponse
(
    string? tradeId,
    decimal? exchangeValue,
    IEnumerable<string>? cardImageUrl,
    decimal? cardAmount,
    decimal? exchangeRate,
    DateTime? tradeDateTime,
    TradeType? tradeType,
    DateTime? validUntil,
    string? otherDetails,
    GiftCardResponseDto? giftCardDetails,
    string? walletId,
    TradingStatus? status
    //Consider adding card and wallet details
);