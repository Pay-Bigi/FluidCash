using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface ITradingServices
{
    Task<StandardResponse<WalletTradingResponse>>
        BuyGiftCardAsync(BuyGiftCardDto buyGiftCardDto, string userId);

    Task<StandardResponse<string>>
        SellGiftCardAsync(SellGiftCardDto sellGiftCardDto, string userId);

    Task<StandardResponse<string>>
        ApproveGiftCardSellAsync(string tradeId, string userId);

    Task<StandardResponse<WalletTradingResponse>>
        GetTradingsAsync(GetTradingsDto getTradingsDto, string userId);

    Task<StandardResponse<string>>
        DeleteTradeAync(string tradeId, string userId);
}
