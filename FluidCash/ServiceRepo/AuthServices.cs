﻿using FluidCash.DataAccess.DbContext;
using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using IEmailSender = FluidCash.IExternalServicesRepo.IEmailSender;

namespace FluidCash.ServiceRepo;

public class AuthServices : IAuthServices
{
    private readonly IAccountMgtServices _accountMgtServices;
    private readonly DataContext _dataContext;
    private readonly ICloudinaryServices _cloudinaryServices;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IEmailSender _emailSender;
    const string tokenProvider = "NumericPasswordReset";
    const string resetPasswordPurpose = "ResetPassword";

    public AuthServices(IAccountMgtServices accountMgtServices, ICloudinaryServices cloudinaryServices,
        UserManager<AppUser> userManager, ITokenService tokenService, IRedisCacheService redisCacheService,
        IEmailSender emailSender, DataContext dataContext)
    {
        _accountMgtServices = accountMgtServices;
        _cloudinaryServices = cloudinaryServices;
        _userManager = userManager;
        _tokenService = tokenService;
        _redisCacheService = redisCacheService;
        _emailSender = emailSender;
        _dataContext = dataContext;
    }

    public async Task<StandardResponse<string>> CreateAccountAsync(CreateAccountParams createAccountDto)
    {
        bool userExists = await _userManager.FindByEmailAsync(createAccountDto.userEmail) is not null;
        if (userExists)
        {
            string errorMsg = "User already exists";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        using var transaction =  await _dataContext.Database.BeginTransactionAsync(); // Start transaction

        try
        {
            var appUser = new AppUser
            {
                Email = createAccountDto.userEmail,
                UserName = createAccountDto.userEmail
            };

            var userCreationResult = await _userManager.CreateAsync(appUser, createAccountDto.password);
            if (!userCreationResult.Succeeded)
            {
                await transaction.RollbackAsync(); // Rollback on failure
                string errorMsg = $"User creation failed.\n {userCreationResult.Errors.FirstOrDefault()}";
                return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
            }

            var userAccount = new CreateUserAccountParams
            {
                displayName = createAccountDto.displayName,
                appUserId = appUser.Id
            };

            // Handle Profile Picture Upload
            if (createAccountDto.dpImage is not null)
            {
                var imageUploadDetails = await _cloudinaryServices.UploadFileToCloudinaryAsync(createAccountDto.dpImage);
                if (imageUploadDetails.Succeeded)
                {
                    userAccount.dpUrl = imageUploadDetails.Data.fileUrlPath;
                    userAccount.dpCloudinaryId = imageUploadDetails.Data.filePublicId;
                }
            }

            var accCreationSuccessful = await _accountMgtServices.CreateUserAccountAsync(userAccount);
            if (!accCreationSuccessful)
            {
                await transaction.RollbackAsync(); // Rollback if account creation fails
                string errorMsg = "An errored while attempting to create account. Kindly retry";
                return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg, statusCode: 500);
            }

            await transaction.CommitAsync(); // Commit transaction if all operations succeed

            string successMsg = "Account successfully created. Proceed to login";
            return StandardResponse<string>.Success(successMsg, statusCode: 201);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Rollback transaction on exception
            string? errorMsg = "An error occurred: " + ex.Message;
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
    }


    public async Task<StandardResponse<string>>
        LoginAsync(LoginParams loginDto)
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
            string errorMsg = "Invalid credentials";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var token = await _userManager.GenerateUserTokenAsync(user, tokenProvider, resetPasswordPurpose);
        var otpUpdateResponse = await ResetAndSendOtpAsync(user.Email, user.UserName, authToken: token);
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
        var confirmOtpResponse = await ConfirmOtp(user.Email, resetPasswordWIthOtpParams.otp);
        if (!confirmOtpResponse)
        {
            errorMsg = "Invalid or expired token";
            return StandardResponse<string>.Failed(errorMsg);
        }
        var isValidToken = await _userManager.VerifyUserTokenAsync(user, tokenProvider, resetPasswordPurpose, resetPasswordWIthOtpParams.otp);

        if (!isValidToken)
        {
            errorMsg = "Invalid OTP";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        // If valid, reset the password manually
        var resetPasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!resetPasswordResult.Succeeded)
        {
            errorMsg = "Failed to remove old password";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordWIthOtpParams.newPassword);
        if (!addPasswordResult.Succeeded)
        {
            errorMsg = "Failed to set new password";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }

        string successMsg = "Password reset successful. Kindly proceed to login";
        return StandardResponse<string>.Success(data: successMsg);
    }

    public async Task<StandardResponse<string>>
        SetTransactionPasswordAsync
        (string userEmail)
    {
        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser is null)
        {
            string errorMsg = "Invalid credentials";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        string token = string.Empty;
        var otpUpdateResponse = await ResetAndSendOtpAsync(appUser.Email, appUser.UserName, token);
        return otpUpdateResponse;
    }

    public async Task<StandardResponse<string>>
        SetTransactionPasswordWithOtpAsync
        (SetTransactionPasswordWithOtpParams passwordParams, string? userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            string errorMsg = "Invalid user Address";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var otpConfirmed = await _redisCacheService.ExistsAsync(userId);
        if (!otpConfirmed)
        {
            string? errorMsg = "Invalid or expired token";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var pwHasher = new PasswordHasher<string>();
        string hashedPassword = pwHasher.HashPassword(userId, passwordParams.transactionPassword);
        appUser.HashedTransactionPin = hashedPassword;

        await _userManager.UpdateAsync(appUser);

        string successMsg = "Transaction password successfully set";
        return StandardResponse<string>.Success(data: successMsg);
    }

    public async Task<StandardResponse<bool>>
        VerifyTransactionPasswordAsync
        (string? transactionPassword, string? userId)
    {
        string errorMsg = "Invalid user credentials";
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);
        var pwHasher = new PasswordHasher<string>();

        var verifyPassword = pwHasher.VerifyHashedPassword(userId, user.HashedTransactionPin, transactionPassword);

        if (verifyPassword == PasswordVerificationResult.Failed)
            return StandardResponse<bool>.Failed(data: false, errorMessage: errorMsg);

        return StandardResponse<bool>.Success(data: true);
    }

    #region Private Methods

    //Used to confirm otp sent to a user. Ensure to persist changes on calling method. 
    private async Task<bool>
        ConfirmOtp
        (string userMail, string otp)
    {
        //confirm OTP from Redis using encrypted user mail as key
        var savedOtp = await _redisCacheService.GetAsync<string>(userMail);
        bool otoExists = otp == savedOtp;
        var otpRemoved = await _redisCacheService.RemoveAsync(userMail);
        return otoExists;
    }

    private async Task<StandardResponse<string>>
       ResetAndSendOtpAsync
       (string? userEmail, string? userName, string? authToken)
    {
        string successMsg = string.Empty;
        string errorMsg = string.Empty;
        authToken = await ResetOtpTrackers(userEmail, authToken);
        if (string.IsNullOrWhiteSpace(authToken))
        {
            errorMsg = "Failed to generate OTP. Request failed";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        await SendOTPMailAsync(userEmail, userName, authToken);

        successMsg = "OTP sent successfully. Kindly confirm otp";
        return StandardResponse<string>.Success(data: successMsg);
    }

    //Sends auth mail otp to specified email address. 
    private async Task<StandardResponse<string>>
        SendOTPMailAsync
        (string? userEmail, string? userName, string otp)
    {
        var emailRecipientDetails = new[] { new EmailRecipientInfo(userName, userEmail) };
        string senderEmail = "paybigie@gmail.com";
        string callingEndpoint = "callingEndpoint";
        string? organizationLogoUrl = "organizationLogoUrl";
        string? organizationName = "PayBigi";
        string? organizationWebUrl = "organizationWebUrl";
        string? linkedinHandleUrl = "linkedinHandleUrl";
        string? twitterHandleUrl = "twitterHandleUrl";
        string? mailSubject = "Email Confirmation";
        string? mailBody = $"Kindly use the otp: {otp} to complete your request.\n The otp expires after 5 minutes.";
        bool isHtml = true;
        string? mailTemplateName = string.Empty;
        ICollection<EmailRecipientInfo>? mailRecipientsInfo = emailRecipientDetails;
        string[]? fileAttachmentPaths = null;


        var mailToUser = new SendEmailParams
        {
            senderEmail = senderEmail,
            callingEndpoint = callingEndpoint,
            organizationLogoUrl = organizationLogoUrl,
            organizationName = organizationName,
            organizationWebUrl = organizationWebUrl,
            linkedinHandleUrl = linkedinHandleUrl,
            twitterHandleUrl = twitterHandleUrl,
            mailSubject = mailSubject,
            mailBody = mailBody,
            isHtml = isHtml,
            mailTemplateName = mailTemplateName,
            mailRecipientsInfo = mailRecipientsInfo,
            fileAttachmentPaths = fileAttachmentPaths
        };
        var sentMailResponse = await _emailSender.ProcessAndSendEmailAsync(mailToUser);

        return sentMailResponse;
    }

    //Resets and sets new otp details for a specified user. Ensure to persist chnages on calling method
    private async Task<string>
        ResetOtpTrackers
        (string userEmail, string? authToken)
    {
        if (string.IsNullOrWhiteSpace(authToken))
        {
            authToken = GenerateOtp();
        }
        //Cache OTP in Redis using encrypted user mail as key
        var isCached = await _redisCacheService.SetAsync(userEmail, authToken, TimeSpan.FromMinutes(5));
        if (!isCached)
        {
            return string.Empty;
        }
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

    #endregion
}
