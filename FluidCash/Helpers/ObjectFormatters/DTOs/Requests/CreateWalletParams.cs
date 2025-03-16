namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record CreateWalletParams
(
    string? currency,
    decimal balance,
    string? accountId
);