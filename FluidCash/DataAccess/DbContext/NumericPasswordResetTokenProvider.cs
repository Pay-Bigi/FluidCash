using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FluidCash.DataAccess.DbContext;

public class NumericPasswordResetTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
{
    public override Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var random = new Random();
        var token = random.Next(10000, 99999).ToString(); // Generates a 5-digit number
        return Task.FromResult(token);
    }

    public override Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        // Basic numeric validation — adjust this if you want more complex rules
        return Task.FromResult(int.TryParse(token, out _));
    }

    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        // Allow generation of the token for any user
        return Task.FromResult(true);
    }
}
