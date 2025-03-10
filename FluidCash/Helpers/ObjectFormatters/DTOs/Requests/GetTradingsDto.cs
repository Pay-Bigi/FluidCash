using FluidCash.Helpers.Enums;

namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record GetTradingsDto
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
    string? giftCardId
);
