namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record TransactionPasswordParams
(
    string? userId,
    string? transactionPassword
);
