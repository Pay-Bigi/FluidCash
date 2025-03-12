using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.Enums;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;

namespace FluidCash.ServiceRepo;

public sealed class TradingServices : ITradingServices
{
    private readonly IBaseRepo<WalletTrading> _tradingRepo;
    private readonly IBaseRepo<Wallet> _walletRepo;

    public TradingServices(IBaseRepo<WalletTrading> tradingRepo, IBaseRepo<Wallet> walletRepo)
    {
        _tradingRepo = tradingRepo;
        _walletRepo = walletRepo;
    }

    public async Task<StandardResponse<string>> 
        ApproveGiftCardSellAsync
        (ApproveGiftCardDto approveGiftCardDto, string userId)
    {
        var trade = _tradingRepo.GetByCondition(x => x.Id == approveGiftCardDto.tradeId).FirstOrDefault();
        if (trade == null)
        {
            string? errorMessage = "Trade not found";
            return StandardResponse<string>.Failed(null, errorMessage);
        }
        if (trade.Status == TradingStatus.Approved)
        {
            string? errorMessage = "Trade already approved";
            return StandardResponse<string>.Failed(null, errorMessage);
        }
        trade.Status = approveGiftCardDto.isApproved ? TradingStatus.Approved : TradingStatus.Declined;
        trade.UpdatedAt = DateTime.UtcNow;
        trade.UpdatedBy = userId;

        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade status updated successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    public async Task<StandardResponse<WalletTradingResponse>> 
        BuyGiftCardAsync
        (BuyGiftCardDto buyGiftCardDto, string userId)
    {
        var cardToBuyExists = await _tradingRepo.ExistsByCondition(x => x.Id == buyGiftCardDto.giftCardId);

        if (!cardToBuyExists)
        {
            string? errorMessage = "Gift card not found";
            return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
        }
        var walletExists = await _walletRepo.ExistsByCondition(x => x.Id == buyGiftCardDto.walletId);

        if (!walletExists)
        {
            string? errorMessage = "Trading wallet not found";
            return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
        }
        var trade = new WalletTrading
        {
            Status = TradingStatus.Pending,
            Type = TradeType.Buy,
            CardAmount = buyGiftCardDto.amount,
            GiftCardId = buyGiftCardDto.giftCardId,
            WalletId = buyGiftCardDto.walletId
        };
        var transaction = new WalletTransaction
        {
            Amount = buyGiftCardDto.amount,
            TransactionReference = trade.Id,
            Type = TransactionType.GiftCardPurchase,
            WalletId = buyGiftCardDto.walletId,
            TradingId = trade.Id
        };
        await _tradingRepo.AddAsync(trade);
        await _walletRepo.AddAsync(transaction);
        await _tradingRepo.SaveChangesAsync();
    }

    public Task<StandardResponse<string>> 
        DeleteTradeAync
        (string tradeId, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<IEnumerable<WalletTradingResponse>>> 
        GetAllTradingsAsync
        (GetTradingsDto getTradingsDto)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<IEnumerable<WalletTradingResponse>>> 
        GetTradingsAsync
        (GetTradingsDto getTradingsDto, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<StandardResponse<string>> 
        SellGiftCardAsync
        (SellGiftCardDto sellGiftCardDto, string userId)
    {
        throw new NotImplementedException();
    }
}
