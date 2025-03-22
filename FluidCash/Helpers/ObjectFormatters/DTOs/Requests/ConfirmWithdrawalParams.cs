namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record ConfirmWithdrawalParams
(
    string? accountId,
    string? otp,
    decimal? amount
);
