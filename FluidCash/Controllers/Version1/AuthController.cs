using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.IServiceRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluidCash.Controllers.Version1;

public class AuthController:V1BaseController
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }

    [AllowAnonymous]
    [HttpPost("create-account")]
    public async Task<IActionResult> CreateAccountAsync
        ([FromForm] CreateAccountParams createAccountDto)
    {
        var response = await _authServices.CreateAccountAsync(createAccountDto);
        return StatusCode(response.StatusCode, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync
        ([FromBody] LoginParams loginDto)
    {
        var response = await _authServices.LoginAsync(loginDto);
        return StatusCode(response.StatusCode, response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync
        ([FromBody] string userEmail)
    {
        var response = await _authServices.ResetPasswordAsync(userEmail);
        return StatusCode(response.StatusCode, response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password-with-otp")]
    public async Task<IActionResult> ResetPasswordWIthOtpAsync
        ([FromBody] ResetPasswordWIthOtpParams resetPasswordWIthOtpParams)
    {
        var response = await _authServices.ResetPasswordWIthOtpAsync(resetPasswordWIthOtpParams);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPost("set-transaction-password")]
    public async Task<IActionResult> SetTransactionPasswordAsync
        ([FromForm] string userEmail)
    {
        var response = await _authServices.SetTransactionPasswordAsync(userEmail);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("set-transaction-password-with-otp")]
    public async Task<IActionResult> SetTransactionPasswordWithOtpAsync
        ([FromBody] SetTransactionPasswordWithOtpParams setTransactionPasswordWithOtpParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _authServices.SetTransactionPasswordWithOtpAsync(setTransactionPasswordWithOtpParams, userId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("verify-transaction-password")]
    public async Task<IActionResult> VerifyTransactionPasswordAsync
        (string? transactionPassword)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _authServices.VerifyTransactionPasswordAsync(transactionPassword, userId);
        return StatusCode(response.StatusCode, response);
    }
}
