using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace FluidCash.ServiceRepo;

public class AuthServices : IAuthServices
{
    private readonly IBaseRepo<Account> _accountRepo;
    private readonly ICloudinaryServices _cloudinaryServices;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthServices(IBaseRepo<Account> accountRepo, ICloudinaryServices cloudinaryServices,
        UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _accountRepo = accountRepo;
        _cloudinaryServices = cloudinaryServices;
        _userManager = userManager;
        _tokenService = tokenService;
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
                var token = await _tokenService.CreateTokenAsync(user);
                string successMsg = "Login successful";
                return StandardResponse<string>.Success(data: token, successMessage: successMsg);
            }
        }
        string errorMsg = "Invalid user credentials";
        return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
    }

    public async Task<StandardResponse<string>>
    ResetPasswordAsync(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            string errorMsg = "Incorrect auth credentials";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var token = await _userManager.GenerateUserTokenAsync(user, "NumericPasswordReset", "ResetPassword");
        var otpUpdateResponse = await ResetAndUpdateOtpAsync(user, authToken: token);
        return otpUpdateResponse;
    }

    public async Task<StandardResponse<string>>
        ResetPasswordWIthOtpAsync
        (ResetPasswordWIthOtpParams resetPasswordWIthOtpParams)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordWIthOtpParams.userEmail);
        string? errorMsg = string.Empty;
        if (user is null)
        {
            errorMsg = "Incorrect auth credentials";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var confirmOtpResponse = ConfirmOtp(user.Email, resetPasswordWIthOtpParams.otp);
        if (!confirmOtpResponse)
        {
            errorMsg = "Invalid or expired token";
            return StandardResponse<string>.Failed(errorMsg);
        }
        var passwordResetResponse = await _userManager.ResetPasswordAsync(user, token: resetPasswordWIthOtpParams.otp, newPassword: resetPasswordWIthOtpParams.newPassword);
        if (!passwordResetResponse.Succeeded)
        {
            errorMsg = passwordResetResponse.Errors.FirstOrDefault().ToString();
            return StandardResponse<string>.Failed(errorMsg);
        }
        string successMsg = "Password reset successful. Kindly proceed to login";
        return StandardResponse<string>.Success(data: successMsg);
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

    //Used to confirm otp sent to a user. Ensure to persist changes on calling method. 
    private bool
        ConfirmOtp
        (string userMail, string otp)
    {
        //confirm OTP from Redis using encrypted user mail as key
        //Check expiry time of OTP

        bool isOtpValid = true;
        return isOtpValid;
    }

    private async Task<StandardResponse<string>>
       ResetAndUpdateOtpAsync
       (AppUser? appUser, string? authToken)
    {
        string successMsg = string.Empty;
        string errorMsg = string.Empty;
        if (appUser is null)
        {
            errorMsg = "Request failed. Invalid credentials";
            return StandardResponse<string>.Failed(data: null, errorMsg);
        }
        authToken = ResetOtpTrackers(appUser.Id, authToken);
        await SendOTPMailAsync(appUser.UserName, appUser.Email, authToken);

        await _userManager.UpdateAsync(appUser);
        successMsg = "OTP sent successfully. Kindly confirm otp";
        return StandardResponse<string>.Success(data: successMsg);
    }


    //Resets and sets new otp details for a specified user. Ensure to persist chnages on calling method
    private string
        ResetOtpTrackers
        (string userId, string? authToken)
    {
        if (string.IsNullOrWhiteSpace(authToken))
        {
            authToken = GenerateOtp();
        }
        //Cache OTP in Redis using encrypted user mail as key
       
        return authToken;
    }

    //Generates random digits for otp requests.
    private string
        GenerateOtp()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();

        byte[] randomNumber = new byte[5]; // Buffer to hold random bytes
        rng.GetBytes(randomNumber); // Fill buffer with secure random bytes

        // Convert the byte array to an integer
        int randomValue = Math.Abs(BitConverter.ToInt32(randomNumber, 0));

        // Get the first 4 digits of the random number
        string otp = (randomValue % 10000).ToString("D5");
        return otp;
    }
}
