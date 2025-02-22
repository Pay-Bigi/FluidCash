using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;

namespace FluidCash.ServiceRepo;

public class AuthServices : IAuthServices
{
    private readonly IBaseRepo<Account> _accountRepo;
    private readonly ICloudinaryServices _cloudinaryServices;
    private readonly UserManager<AppUser> _userManager;

    public AuthServices(IBaseRepo<Account> accountRepo, ICloudinaryServices cloudinaryServices,
        UserManager<AppUser> userManager)
    {
        _accountRepo = accountRepo;
        _cloudinaryServices = cloudinaryServices;
        _userManager = userManager;
    }

    public async Task<StandardResponse<string>>
        CreateAccountAsync(CreateAccountDto createAccountDto)
    {
        var appUser = new AppUser
        {
            Email = createAccountDto.userEmail,
            UserName = createAccountDto.userEmail
        };
        var userAccount = new Account
        {
            DisplayName = createAccountDto.displayName,
            AppUserId = appUser.Id
        };
        if (createAccountDto.dpImage is not null)
        {
            var imageFile = createAccountDto.dpImage;
            var imageUploadDetails = await _cloudinaryServices.UploadFileToCloudinaryAsync(imageFile);
            if (imageUploadDetails.Status)
            {
                userAccount.DpUrl = imageUploadDetails.Data.fileUrlPath;
                userAccount.DpCloudinaryId = imageUploadDetails.Data.filePublicId;
            }
        }
        var userCreationResponse = await _userManager.CreateAsync(appUser, createAccountDto.password);
        await _accountRepo.AddAsync(userAccount);
        await _accountRepo.SaveChangesAsync();

        string? successMsg = "Account successfully created. Proceed to login";
        return  StandardResponse<string>.Success(successMsg, statusCode: 201);
    }

    public async Task<StandardResponse<string>>
        LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.userEmail);
        if (user is not null)
        {
            var signInResult = await _userManager.CheckPasswordAsync(user, loginDto.password);
            if (signInResult)
            {
                string successMsg = "Login successful";
                return StandardResponse<string>.Success(data: , successMessage: successMsg);
            }
        }
        string errorMsg = "Invalid user credentials";
        return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
    }

    public async Task<StandardResponse<string>>
        SetTransactionPasswordWithOtpAsync
        (TransactionPasswordParams passwordParams)
    {
        var appUser = await _userManager.FindByIdAsync(passwordParams.userId);
        if (appUser is null)
        {
            string errorMsg = "Invalid user Address";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        var pwHasher = new PasswordHasher<string>();
        string hashedPassword = pwHasher.HashPassword(passwordParams.userId, passwordParams.transactionPassword);
        appUser.HashedTransactionPin = hashedPassword;

        await _accountRepo.SaveChangesAsync();

        string successMsg = "Transaction password successfully set";
        return StandardResponse<string>.Success(data: successMsg);
    }

    public async Task<StandardResponse<bool>>
        VerifyTransactionPasswordAsync
        (TransactionPasswordParams passwordParams)
    {
        string errorMsg = "Invalid user credentials";
        var user = await _userManager.FindByIdAsync(passwordParams.userId);
        if (user is null)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);
        var pwHasher = new PasswordHasher<string>();

        var verifyPassword = pwHasher.VerifyHashedPassword(passwordParams.userId, user.HashedTransactionPin, passwordParams.transactionPassword);

        if (verifyPassword == PasswordVerificationResult.Failed)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);

        return StandardResponse<bool>.Success(data: true);
    }

}
