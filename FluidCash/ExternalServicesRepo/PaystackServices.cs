using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using PayStack.Net;

namespace FluidCash.ExternalServicesRepo;

public sealed class PaystackServices : IPaystackServices
{
    private readonly PayStackApi _payStackServices;

    public PaystackServices(PayStackApi payStackServices)
    {
        _payStackServices = payStackServices;
    }

    public StandardResponse<string>
        InitiateTransfer
        (InitiateTransferParams initiateTransferParams)
    {
        var recipientCode = CreateTransferRecipient(initiateTransferParams.accountName, initiateTransferParams.accountNumber, initiateTransferParams.bankCode);
        if (!recipientCode.Succeeded)
        {
            return recipientCode;
        }
        var transferReponse = _payStackServices.Transfers.InitiateTransfer(initiateTransferParams.amount, recipientCode.Data);

        return StandardResponse<string>.Success(data: transferReponse.Message);
    }

    public StandardResponse<string>
        ResolveBankAccount
        (ResolveAccountParams resolveAccountParams)
    {
        var accountDetails = _payStackServices.Miscellaneous.ResolveAccountNumber(accountNumber: resolveAccountParams.accountNumber, bankCode: resolveAccountParams.bankCode);
        string errorMsg = "Account number appears invalid, kindly confirm bank and account name";
        if (accountDetails.Status)
        {
            string accountName = accountDetails.Data.AccountName ?? errorMsg;

            return StandardResponse<string>.Success(data: accountName);
        }
        return StandardResponse<string>.Failed(data: errorMsg);
    }

    public StandardResponse<InitiateTransactionResponse>
        InitiateTransaction
        (InitializePaymentParams initializePaymentParams)
    {
        var transactionRef = GenerateTransactionRef();
        var payStackResponse = _payStackServices.Transactions.Initialize
            (
                email: initializePaymentParams.clientMail,
                amountInKobo: (int)(initializePaymentParams.amount*100),
                reference: transactionRef, makeReferenceUnique: false);

        var errorMsg = $"Failed to initialize transactiuon at the moment. Provider details: {payStackResponse.Message}";

        if (payStackResponse.Status != true)
        {
            return StandardResponse<InitiateTransactionResponse>.Failed(null, errorMsg);
        }

        if (string.IsNullOrWhiteSpace(payStackResponse.Data.AuthorizationUrl))
        {
            return StandardResponse<InitiateTransactionResponse>.Failed(data: null, errorMessage: errorMsg, statusCode: 500);
        }
        var transactionAuthUrl = payStackResponse.Data.AuthorizationUrl;
        transactionRef = payStackResponse.Data.Reference;

        var initiateTransactionResponse = new InitiateTransactionResponse(transactionAuthUrl, transactionRef);
        return StandardResponse<InitiateTransactionResponse>.Success(data: initiateTransactionResponse);
    }

    public StandardResponse<VerifyPayStackPayment>
        VerifyTransaction
        (string transactionRef)
    {
        var payStackVerfication = _payStackServices.Transactions.Verify(transactionRef);
        if (!payStackVerfication.Status)
        {
            return StandardResponse<VerifyPayStackPayment>.Failed(data: null, errorMessage: payStackVerfication.Message);
        }
        var payStackData = payStackVerfication.Data;
        var verifyPayStackPaymentResp = new VerifyPayStackPayment
             (
                status: payStackData.Status,
                authorizationCode: payStackData.Authorization.AuthorizationCode,
                amount: payStackData.Amount,
                transactionReference: payStackData.Reference,
                currency: payStackData.Currency,
                bank: payStackData.Authorization.Bank,
                channel: payStackData.Authorization.Channel,
                cardType: payStackData.Authorization.CardType,
                paidAt: payStackData.TransactionDate,
                customerMail: payStackData.Customer.Email
            );
        return StandardResponse<VerifyPayStackPayment>.Success(verifyPayStackPaymentResp);
    }

    public StandardResponse<IEnumerable<BankListResponse>>
        GetBankList()
    {
        var bankApiResponse = _payStackServices.Miscellaneous.ListBanks();

        if (!bankApiResponse.Status)
        {
            string errorMsg = "Network currently unavailable. Kindly retry";
            return StandardResponse<IEnumerable<BankListResponse>>.UnExpectedError(data: null);
        }

        var bankList = bankApiResponse.Data.Select(bank => new BankListResponse
        (
            bankName: bank.Name,
            bankCode: bank.Code
        ));

        return StandardResponse<IEnumerable<BankListResponse>>.Success(bankList);
    }

    private StandardResponse<string>
        CreateTransferRecipient
        (string? accName, string? accNo, string? bankCode)
    {
        var recipientDetails = _payStackServices.Transfers.Recipients.Create
            (
                name: accName,
                accountNumber: accNo,
                bankCode: bankCode
            );
        if (recipientDetails.Status)
        {
            string recipientCode = recipientDetails.Data.RecipientCode;

            return StandardResponse<string>.Success(data: recipientCode);
        }
        return StandardResponse<string>.UnExpectedError(data: null);
    }

    private string
        GenerateTransactionRef()
    {
        return Ulid.NewUlid()
            .ToString().Substring(0, 25);
    }
}

