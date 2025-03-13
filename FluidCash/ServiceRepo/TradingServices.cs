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
    private readonly IBaseRepo<WalletTransaction> _transactionRepo;
    private readonly IGiftCardServices _giftCardServices;

    public TradingServices(IBaseRepo<WalletTrading> tradingRepo, IBaseRepo<Wallet> walletRepo,
        IBaseRepo<WalletTransaction> transactionRepo, IGiftCardServices giftCardServices)
    {
        _tradingRepo = tradingRepo;
        _walletRepo = walletRepo;
        _transactionRepo = transactionRepo;
        _giftCardServices = giftCardServices;
    }

    public async Task<StandardResponse<string>> 
        ApproveGiftCardSellAsync
        (ApproveGiftCardDto approveGiftCardDto, string userId)
    {
        var trade = _tradingRepo.GetByCondition(x => x.Id == approveGiftCardDto.tradeId).FirstOrDefault();
        if (trade is null)
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
        var cardToBuyExists = await _giftCardServices.ConfirmCardExistsAsync(buyGiftCardDto.giftCardId);

        if (!cardToBuyExists)
        {
            string? errorMessage = "Card currently unavailable. Try again later";
            return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
        }
        var walletExists = await _walletRepo.ExistsByCondition(x => x.Id == buyGiftCardDto.walletId);

        if (!walletExists)
        {
            string? errorMessage = "Invalid wallet account";
            return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
        }
        var exchangeRate = await _giftCardServices.GetGiftCardRateByIdAsync(buyGiftCardDto.giftCardRateId);
        var trade = new WalletTrading
        {
            Status = TradingStatus.Pending,
            Type = TradeType.Buy,
            ExchangeValue = exchangeRate * buyGiftCardDto.amount,
            CardAmount = buyGiftCardDto.amount,
            ExchangeRate = exchangeRate,
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
        await _transactionRepo.AddAsync(transaction);
        await _tradingRepo.SaveChangesAsync();

        var exchangeValue = trade.ExchangeValue;
        var tradeResponse = new WalletTradingResponse
        (
            tradeId: trade.Id,
            exchangeValue: trade.ExchangeValue,
            cardImageUrl: trade.CardImageUrl,
            cardAmount: trade.CardAmount,
            exchangeRate: trade.ExchangeRate,
            tradeDateTime: trade.CreatedAt,
            tradeType: trade.Type,
            validUntil: trade.ValidUntil,
            otherDetails: trade.OtherDetails,
            giftCardId: trade.GiftCardId,
            walletId: trade.WalletId
        );
        return StandardResponse<WalletTradingResponse>.Success(tradeResponse);
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
