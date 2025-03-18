using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;

namespace FluidCash.ServiceRepo;

public sealed class AccountMgtServices : IAccountMgtServices
{
    private readonly IBaseRepo<Account> _accountRepo;
    private readonly IWalletServices _walletServices;

    public AccountMgtServices(IBaseRepo<Account> accountRepo, IWalletServices walletServices)
    {
        _accountRepo = accountRepo;
        _walletServices = walletServices;
    }


    public async Task<bool>
        CreateUserAccountAsync
        (CreateUserAccountParams createUserAccountDto)
    {
        try
        {
            var userAccount = new Account
            {
                DisplayName = createUserAccountDto.displayName,
                AppUserId = createUserAccountDto.appUserId,
                DpCloudinaryId = createUserAccountDto.dpCloudinaryId,
                DpUrl = createUserAccountDto.dpUrl,
                CreatedAt = DateTime.Now,
                CreatedBy = createUserAccountDto.appUserId
            };
            //Create Account Wallet
            var walletCreationParams = new CreateWalletParams
            (
                currency: "NGN",
                balance: 0,
                accountId: userAccount.Id
            );
            var walletId = await _walletServices.CreateWalletAsync(walletCreationParams, userAccount.AppUserId);
            userAccount.WalleId = walletId.Data;
            await _accountRepo.AddAsync(userAccount);
            await _accountRepo.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public Task<StandardResponse<string>> 
        DeleteDpAsync
        (string accountId, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<AccountResponseDto>> 
        GetUserAccountsAsync
        (string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<IEnumerable<AccountResponseDto>>>
        GetAllAccountsAsync(AccountsFilterParams accountsFilterParams)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<DashboardResponse>> 
        GetUserDashboardAsync
        (string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> 
        InitiateWithdrawalAsync
        (InitiateWithdrawalParams withdrawalParams, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> 
        ConfirmWithdrawalAsync
        (ConfirmWithdrawalParams confirmWithdrawalParams, string? userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> 
        UpdateBankDetails
        (BankDetailsDto bankDetailsDto, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> 
        UploadDpAsync
        (UploadDpParams uploadDpParams, string? userId)
    {
        throw new NotImplementedException();
    }
}
