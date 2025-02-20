namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record TransactionPasswordParams
(
    string? walletId,
    string? transactionPassword
);
