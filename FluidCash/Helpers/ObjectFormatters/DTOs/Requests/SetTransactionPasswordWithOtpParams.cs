namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record SetTransactionPasswordWithOtpParams
(
    string? otp,
    string? transactionPassword
);
