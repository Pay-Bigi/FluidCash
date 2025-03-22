namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public record BankDetailsDto
(
    string? accountId,
    string? accountNumber,
    string? accountName,
    string? bankCode
);