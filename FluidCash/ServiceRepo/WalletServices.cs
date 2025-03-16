using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;

namespace FluidCash.ServiceRepo;

public sealed class WalletServices : IWalletServices
{
    private readonly IBaseRepo<Wallet> _walletRepo;

    public WalletServices(IBaseRepo<Wallet> walletRepo)
    {
        _walletRepo = walletRepo;
    }

    public async Task<bool> 
        ConfirmWalletExistsAsync(string? walletId)
    {
        bool walletExists = await _walletRepo.ExistsByConditionAsync(wallet=>wallet.Id == walletId);
        return walletExists;
    }

    public async Task<StandardResponse<string>> 
        CreateWalletAsync(CreateWalletParams createWalletDto, string userId)
    {
        var walletToCreate = new Wallet
        {
            AccountId = createWalletDto.accountId,
            Balance = createWalletDto.balance,
            Currency = createWalletDto.currency,
            CreatedBy = userId,
            CreatedAt = DateTime.Now
        };
        await _walletRepo.AddAsync(walletToCreate);
        string? walletId = walletToCreate.Id;
        return StandardResponse<string>.Success(walletId);
    }

    public async Task<bool> 
        CreditWalletAsync(CreditAndDebitWalletParams? creditWalletParams, string? userId)
    {
        var accountToCredit = _walletRepo.GetNonDeletedByCondition(wallet => wallet.Id == creditWalletParams.walletId)
            .FirstOrDefault();
        if(accountToCredit is null)
        {
            return false;
        }
        accountToCredit.Balance += creditWalletParams.amount;
        await _walletRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DebitWalletAsync(CreditAndDebitWalletParams? debitWalletParams, string? userId)
    {
        var accountToCredit = _walletRepo.GetNonDeletedByCondition(wallet => wallet.Id == debitWalletParams.walletId)
            .FirstOrDefault();
        if (accountToCredit is null)
        {
            return false;
        }
        accountToCredit.Balance -= debitWalletParams.amount;
        await _walletRepo.SaveChangesAsync();
        return true;
    }

    public Task<StandardResponse<IEnumerable<WalletResponseDto>>> GetAllWalletsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<WalletResponseDto>> GetUserWalletAsync(string? walletId, string? userId)
    {
        throw new NotImplementedException();
    }
}
