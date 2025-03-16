namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record WalletResponseDto
(
    string? currency,
    decimal? balance,
    IEnumerable<WalletTransactionResponse> walletTransactions
);