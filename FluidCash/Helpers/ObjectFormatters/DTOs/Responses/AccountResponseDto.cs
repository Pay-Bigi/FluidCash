namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public record AccountResponseDto
(
    string? accountId,
    string? userEmail,
    string? displayName,
    string? dpUrl,
    BankDetailsResponse? bankDetails
);

public record BankDetailsResponse
(
    string? bankCode,
    string? accountName,
    string? accountNumber
);