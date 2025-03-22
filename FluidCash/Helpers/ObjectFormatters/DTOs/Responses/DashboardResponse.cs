namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record DashboardResponse
(
    string? accountId,
    string? userEmail,
    string? displayName,
    string? dpUrl,
    string? walletId,
    string? currency,
    decimal? balance,
    IEnumerable<WalletTransactionResponse>? todaysTransactions
);
