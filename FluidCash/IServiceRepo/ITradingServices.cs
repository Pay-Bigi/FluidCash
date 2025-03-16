using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface ITradingServices
{
    Task<StandardResponse<WalletTradingResponse>>
        BuyGiftCardAsync(BuyGiftCardParams buyGiftCardDto, string userId);

    Task<StandardResponse<WalletTradingResponse>>
        SellGiftCardAsync(SellGiftCardParams sellGiftCardDto, string userId);

    Task<StandardResponse<string>>
        ApproveGiftCardSellAsync(ApproveGiftCardSellParams approveGiftCardDto, string userId);

    Task<StandardResponse<string>>
        ApproveGiftCardPurchaseAsync(ApproveGiftCardPurchaseParams approveGiftCardPurchaseDto, string? userId);

    Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetUserTradingsAsync(GetTradingsFilterParams getTradingsDto, string userId);

    Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetAllTradingsAsync(GetTradingsFilterParams getTradingsDto);

    Task<StandardResponse<string>>
        DeleteTradeAync(string tradeId, string userId);
}
