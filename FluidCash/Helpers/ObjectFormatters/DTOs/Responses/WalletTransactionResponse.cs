using FluidCash.Helpers.Enums;

namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record WalletTransactionResponse
(
    string? trnsactionId,
    TransactionType transactionType,
    string? transactionReference,
    decimal? amount,
    string? otherDetails,
    WalletTradingResponse? walletTradingResponse
);