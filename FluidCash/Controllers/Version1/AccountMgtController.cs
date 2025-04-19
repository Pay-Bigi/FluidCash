using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.IServiceRepo;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluidCash.Controllers.Version1;

public class AccountMgtController:V1BaseController
{
    private readonly IAccountMgtServices _accountMgtServices;

    public AccountMgtController(IAccountMgtServices accountMgtServices)
    {
        _accountMgtServices = accountMgtServices;
    }

    [HttpPost("create-user-bank-details")]
    public async Task<IActionResult> CreateBankDetails
        ([FromBody] BankDetailsDto bankDetailsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.CreateBankDetails(bankDetailsDto, userId!);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("get-all-accounts")]
    public async Task<IActionResult> GetAllAccountsAsync
        ([FromQuery] AccountsFilterParams accountsFilterParams)
    {
        var response = await _accountMgtServices.GetAllAccountsAsync(accountsFilterParams);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("get-user-dashboard")]
    public async Task<IActionResult> GetUserDashboardAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.GetUserDashboardAsync(userId!);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("get-user-account")]
    public async Task<IActionResult> GetUserAccountAsync
        ([FromQuery] string accountId)
    {
        var response = await _accountMgtServices.GetUserAccountAsync(accountId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("upload-user-dp")]
    public async Task<IActionResult> UploadDpAsync
        ([FromForm] UploadDpParams uploadDpParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.UploadDpAsync(uploadDpParams, userId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("delete-user-dp")]
    public async Task<IActionResult> DeleteDpAsync
        ([FromQuery] string accountId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.DeleteDpAsync(accountId, userId!);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("update-user-bank-details")]
    public async Task<IActionResult> UpdateBankDetailsAsync
        ([FromBody] BankDetailsDto bankDetailsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.UpdateBankDetails(bankDetailsDto, userId!);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("initiate-withdrawal")]
    public async Task<IActionResult> InitiateWithdrawalAsync
        ([FromBody] InitiateWithdrawalParams withdrawalParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.InitiateWithdrawalAsync(withdrawalParams, userId!);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("confirm-withdrawal")]
    public async Task<IActionResult> ConfirmWithdrawalAsync
        ([FromBody] ConfirmWithdrawalParams confirmWithdrawalParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _accountMgtServices.ConfirmWithdrawalAsync(confirmWithdrawalParams, userId);
        return StatusCode(response.StatusCode, response);
    }
}
