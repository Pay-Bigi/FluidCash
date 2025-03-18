namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record InitiateWithdrawalParams
(
    string? accountId,
    string? transactionPin,
    decimal? amount
);
