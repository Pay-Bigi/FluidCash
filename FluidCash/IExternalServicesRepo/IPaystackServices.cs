using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IExternalServicesRepo;

public interface IPaystackServices
{
    StandardResponse<string>
        InitializeTransfer
        (InitiateTransferParams initiateTransferParams);

    StandardResponse<string>
        ResolveBankAccount
        (ResolveAccountParams resolveAccountParams);

    StandardResponse<InitiateTransactionResponse>
        InitiateTransaction
        (InitializePaymentParams initializePaymentParams);

    StandardResponse<VerifyPayStackPayment>
        VerifyTransaction
        (string transactionRef);

    StandardResponse<IEnumerable<BankListResponse>>
        GetBankList();
}
