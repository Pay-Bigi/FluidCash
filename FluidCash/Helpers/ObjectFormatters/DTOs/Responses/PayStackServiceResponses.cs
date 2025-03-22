namespace FluidCash.Helpers.ObjectFormatters.DTOs.Responses;

public class PayStackServiceResponses
{
}

public record InitiateTransactionResponse
(
    string transactionAuthUrl,
    string transactionReference
);

public record BankListResponse
(string? bankName, string? bankCode);

public record VerifyPayStackPayment
(
    string? status,
    string? authorizationCode,
    int amount,
    string? transactionReference,
    string? currency,
    string? bank,
    string? channel,
    string? cardType,
    DateTime? paidAt,
    string? customerMail
);