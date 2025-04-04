﻿using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IAuthServices
{
    Task<StandardResponse<string>>
        CreateAccountAsync(CreateAccountParams createAccountDto);

    Task<StandardResponse<string>>
        LoginAsync(LoginParams loginDto);

    Task<StandardResponse<string>>
    ResetPasswordAsync(string userEmail);

    Task<StandardResponse<string>>
        ResetPasswordWIthOtpAsync
        (ResetPasswordWIthOtpParams resetPasswordWIthOtpParams);

    Task<StandardResponse<string>>
        SetTransactionPasswordAsync
        (string userEmail);

    Task<StandardResponse<string>>
        SetTransactionPasswordWithOtpAsync
        (SetTransactionPasswordWithOtpParams passwordParams, string? userId);

    Task<StandardResponse<bool>>
        VerifyTransactionPasswordAsync
        (string? transactionPassword, string? userId);
}
