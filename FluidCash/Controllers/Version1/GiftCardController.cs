using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.IServiceRepo;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluidCash.Controllers.Version1;

public class GiftCardController:V1BaseController
{
    private readonly IGiftCardServices _giftCardServices;

    public GiftCardController(IGiftCardServices giftCardServices)
    {
        _giftCardServices = giftCardServices;
    }

    [HttpPost("create-giftcard")]
    public async Task<IActionResult> CreateGiftCardAsync([FromBody] CreateGiftCardAndRateParams createGiftCardAndRateParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _giftCardServices.CreateGiftCardAndRateAsync(createGiftCardAndRateParams, userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("update-giftcard")]
    public async Task<IActionResult> UpdateGiftCardAsync([FromBody] UpdateGiftCardParams updateGiftCardParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _giftCardServices.UpdateGiftCardAsync(updateGiftCardParams, userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("get-giftcard")]
    public async Task<IActionResult> GetGiftCardByIdAsync([FromQuery] string giftCardId)
    {
        var result = await _giftCardServices.GetGiftCardByIdApiAsync(giftCardId);
        return Ok(result);
    }

    [HttpGet("get-giftcards")]
    public async Task<IActionResult> GetGiftCardsAsync([FromQuery] GetGiftCardFilterParams getGiftCardFilterParams)
    {
        var result = await _giftCardServices.GetGiftCardAsync(getGiftCardFilterParams);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("delete-giftcard")]
    public async Task<IActionResult> DeleteGiftCardAsync([FromQuery,] string giftCardId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _giftCardServices.DeleteGiftCardAsync(giftCardId, userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("update-giftcard-rate")]
    public async Task<IActionResult> UpdateGiftCardRateAsync([FromBody] UpdateGiftCardRateParams updateGiftCardRateParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _giftCardServices.UpdateGiftCardRateAsync(updateGiftCardRateParams, userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("get-giftcard-rates")]
    public async Task<IActionResult> GetGiftCardRatesAsync([FromQuery] GetGiftCardRateFilterParams getGiftCardRateFilterParams)
    {
        var result = await _giftCardServices.GetGiftCardRatesAsync(getGiftCardRateFilterParams);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("delete-giftcard-rate")]
    public async Task<IActionResult> DeleteGiftCardRateAsync([FromQuery] string giftCardRateId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _giftCardServices.DeleteGiftCardRateAsync(giftCardRateId, userId);
        return StatusCode(result.StatusCode, result);
    }
}
