using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.ServiceRepo;

public class AuthServices : IAuthServices
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
        var appUser = _appUserRepo.GetNonDeletedByCondition(user => user.Id == passwordParams.userId).FirstOrDefault();
        if (appUser is null)
        {
            string errorMsg = "Invalid user Address";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        var pwHasher = new PasswordHasher<string>();
        string hashedPassword = pwHasher.HashPassword(passwordParams.userId, passwordParams.transactionPassword);
        appUser.HashedTransactionPin = hashedPassword;

        await _appUserRepo.SaveChangesAsync();

        string successMsg = "Transaction password successfully set";
        return StandardResponse<string>.Success(data: successMsg);
    }

    public async Task<StandardResponse<bool>>
        VerifyTransactionPasswordAsync
        (TransactionPasswordParams passwordParams)
    {
        string errorMsg = "Invalid user credentials";
        var user = await _appUserRepo.GetNonDeletedByCondition(wlt => wlt.Id == passwordParams.userId)
            .AsNoTracking().FirstOrDefaultAsync();
        if (user is null)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);
        var pwHasher = new PasswordHasher<string>();

        var verifyPassword = pwHasher.VerifyHashedPassword(passwordParams.userId, user.HashedTransactionPin, passwordParams.transactionPassword);

        if (verifyPassword == PasswordVerificationResult.Failed)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);

        return StandardResponse<bool>.Success(data: true);
    }

}
