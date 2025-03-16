namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreditAndDebitWalletParams
(
    string? walletId,
    decimal amount
);