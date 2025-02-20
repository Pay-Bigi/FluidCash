using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.ServiceRepo;

public class AuthServices:IAuthServices
{
    private readonly IBaseRepo<AppUser> _appUserRepo;

    public AuthServices(IBaseRepo<AppUser> appUserRepo)
    {
        _appUserRepo = appUserRepo;
    }

    public async Task<StandardResponse<string>>
    SetTransactionPasswordWithOtpAsync
    (TransactionPasswordParams passwordParams)
    {
        var appUser = _appUserRepo.GetNonDeletedByCondition(user => user.I == passwordParams.walletId)
            .Include(wlt => wlt.Account)
            .FirstOrDefault();
        if (wallet is null)
        {
            string errorMsg = "Invalid Wallet Address";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        var user = wallet.Account;

        
            var pwHasher = new PasswordHasher<string>();
            string hashedPassword = pwHasher.HashPassword(passwordParams.walletId, passwordParams.transactionPassword);
             = hashedPassword;

            await _appUserRepo.SaveChangesAsync();


            string successMsg = "Transaction password successfully set";
            return StandardResponse<string>.Success(data: successMsg);

    }

}
