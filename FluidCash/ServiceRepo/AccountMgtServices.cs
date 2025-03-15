using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;

namespace FluidCash.ServiceRepo;

public sealed class AccountMgtServices : IAccountMgtServices
{
    private readonly IBaseRepo<Account> _accountRepo;

    public AccountMgtServices(IBaseRepo<Account> accountRepo)
    {
        _accountRepo = accountRepo;
    }

    public async Task<bool>
        CreateUserAccountAsync
        (CreateUserAccountDto createUserAccountDto)
    {
        try
        {
            var userAccount = new Account
            {
                DisplayName = createUserAccountDto.displayName,
                AppUserId = createUserAccountDto.appUserId,
                DpCloudinaryId = createUserAccountDto.dpCloudinaryId,
                DpUrl = createUserAccountDto.dpUrl
            };
            //Create Account Wallet
            await _accountRepo.AddAsync(userAccount);
            await _accountRepo.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
