namespace FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

public class PayStackServiceRequestParams
{
}

public record InitiateTransferParams
(
    int amount,
    string? accountName,
    string? accountNumber,
    string? bankCode
);

public record ResolveAccountParams
(
    string? accountNumber,
    string? bankCode
);

public record InitializePaymentParams
(
    string clientMail,
    int amount
);