using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FluidCash.Controllers.Version1;

public class TradingsController:V1BaseController
{
    private readonly ITradingServices _tradingServices;
    private readonly IInterswitchServices _flutterWaveServices;

    public TradingsController(ITradingServices tradingServices, IInterswitchServices flutterWaveServices)
    {
        _tradingServices = tradingServices;
        _flutterWaveServices = flutterWaveServices;
    }

    [HttpPost("buy-giftcard")]
    public async Task<IActionResult> BuyGiftCardAsync([FromBody] BuyGiftCardParams buyGiftCardParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.BuyGiftCardAsync(buyGiftCardParams,userId!);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("approve-giftcard-purchase")]
    public async Task<IActionResult> ApproveGiftCardPurchaseAsync([FromForm] ApproveGiftCardPurchaseParams approveGiftCardPurchaseParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.ApproveGiftCardPurchaseAsync(approveGiftCardPurchaseParams,userId);
        return StatusCode(result.StatusCode, result);   
    }

    [HttpPost("sell-giftcard")]
    public async Task<IActionResult> SellGiftCardAsync([FromForm] SellGiftCardParams sellGiftCardParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.SellGiftCardAsync(sellGiftCardParams, userId!);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("approve-giftcard-sell")]
    public async Task<IActionResult> ApproveGiftCardSellAsync([FromBody] ApproveGiftCardSellParams approveGiftCardSellParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.ApproveGiftCardSellAsync(approveGiftCardSellParams, userId!);
        return StatusCode(result.StatusCode, result);   
    }

    [HttpGet("user-tradings")]
    public async Task<IActionResult> GetUserTradingsAsync([FromQuery] GetTradingsFilterParams filterParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.GetUserTradingsAsync(filterParams, userId!);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("all-tradings")]
    public async Task<IActionResult> GetAllTradingsAsync([FromQuery] GetTradingsFilterParams filterParams)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.GetAllTradingsAsync(filterParams);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("delete-trade")]
    public async Task<IActionResult> DeleteTradeAsync([FromQuery] string tradeId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _tradingServices.DeleteTradeAync(tradeId, userId!);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpGet("test")]
    public async Task<IActionResult> TestsAync()
    {
        var response = await _flutterWaveServices.RechargeAirtimeAsync(null, null, null);
        return Ok(response);
    }
}
