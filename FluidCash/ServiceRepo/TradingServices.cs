using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.Enums;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FluidCash.ServiceRepo;

public sealed class TradingServices : ITradingServices
{
    private readonly IBaseRepo<WalletTrading> _tradingRepo;
    private readonly IBaseRepo<Wallet> _walletRepo;
    private readonly IBaseRepo<WalletTransaction> _transactionRepo;
    private readonly IGiftCardServices _giftCardServices;
    private readonly ICloudinaryServices _cloudinaryServices;

    public TradingServices(IBaseRepo<WalletTrading> tradingRepo, IBaseRepo<Wallet> walletRepo,
        IBaseRepo<WalletTransaction> transactionRepo, IGiftCardServices giftCardServices,
        ICloudinaryServices cloudinaryServices)
    {
        _tradingRepo = tradingRepo;
        _walletRepo = walletRepo;
        _transactionRepo = transactionRepo;
        _giftCardServices = giftCardServices;
        _cloudinaryServices = cloudinaryServices;
    }
    //Correction Included
    public async Task<StandardResponse<string>>
        ApproveGiftCardSellAsync
        (ApproveGiftCardSellDto approveGiftCardDto, string userId)
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

        //If Approved, process payment to wallet account

        trade.UpdatedAt = DateTime.UtcNow;
        trade.UpdatedBy = userId;

        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade status updated successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    //Correction Included
    public async Task<StandardResponse<string>>
        ApproveGiftCardPurchaseAsync
        (ApproveGiftCardPurchaseDto approveGiftCardPurchaseDto, string? userId)
    {
        var trade = _tradingRepo.GetByCondition(x => x.Id == approveGiftCardPurchaseDto.tradeId).FirstOrDefault();
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
        if (approveGiftCardPurchaseDto.isApproved)
        {
            trade.Status = TradingStatus.Approved;
            trade.ValidUntil = approveGiftCardPurchaseDto.validUntil;
            trade.OtherDetails = approveGiftCardPurchaseDto.otherDetails;
            if (approveGiftCardPurchaseDto.cardImage is not null)
            {
                var imageUploadDetails = await _cloudinaryServices.UploadFileToCloudinaryAsync(approveGiftCardPurchaseDto.cardImage);
                if (!imageUploadDetails.Succeeded)
                {
                    string? errorMsg = "Card image upload failed. Kindly retry";
                    return StandardResponse<string>.Failed(null, errorMsg);
                }
                trade.CardImageUrl = imageUploadDetails.Data.fileUrlPath;
                trade.CardImageId = imageUploadDetails.Data.filePublicId;
            }
        }
        else
        {
            trade.Status = TradingStatus.Declined;
            //Process refund to wallet Account
        }
        trade.UpdatedAt = DateTime.UtcNow;
        trade.UpdatedBy = userId;
        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade status updated successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    //Correction Included
    public async Task<StandardResponse<WalletTradingResponse>>
        BuyGiftCardAsync
        (BuyGiftCardDto buyGiftCardDto, string userId)
    {
        var cardToBuyResponse = await _giftCardServices.GetGiftCardByIdAsync(buyGiftCardDto.giftCardId);

        if (cardToBuyResponse.Succeeded)
        {
            if (cardToBuyResponse.Data is not null)
            {
                var walletExists = await _walletRepo.ExistsByCondition(x => x.Id == buyGiftCardDto.walletId);

                if (!walletExists)
                {
                    string? errorMsg = "Invalid wallet account";
                    return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
                }
                //Process Payment via wallet or paystack
                var exchangeRate = cardToBuyResponse.Data.GiftCardRates
                    .FirstOrDefault(x=>x.giftCardRateId == buyGiftCardDto.giftCardRateId)?.rate;
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
                    giftCardDetails: cardToBuyResponse.Data,
                    walletId: trade.WalletId
                );
                return StandardResponse<WalletTradingResponse>.Success(tradeResponse);
            }
        }
        string? errorMessage = "Card currently unavailable. Try again later";
        return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
    }

    public async Task<StandardResponse<string>>
        DeleteTradeAync
        (string tradeId, string userId)
    {
        var tradeToDelete = _tradingRepo.GetByCondition(x => x.Id == tradeId).FirstOrDefault();
        if (tradeToDelete is null)
        {
            string? errorMessage = "Trade not found";
            return StandardResponse<string>.Failed(null, errorMessage);
        }
        _tradingRepo.SoftDelete(tradeToDelete);
        tradeToDelete.UpdatedBy = userId;
        tradeToDelete.UpdatedAt = DateTime.UtcNow;
        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade deleted successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    public async Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetAllTradingsAsync
        (GetTradingsDto getTradingsDto)
    {
        var query = _tradingRepo.GetAll();
        if (!string.IsNullOrWhiteSpace(getTradingsDto.tradeId))
        {
            var tradeId = getTradingsDto.tradeId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Id.ToLower(), $"{tradeId}"));
        }
        if (!string.IsNullOrWhiteSpace(getTradingsDto.giftCardId))
        {
            var giftCardId = getTradingsDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.GiftCardId.ToLower(), $"{giftCardId}"));
        }
        if (getTradingsDto.exchangeValue.HasValue)
        {
            query = query.Where(x => x.ExchangeValue == getTradingsDto.exchangeValue);
        }
        if (getTradingsDto.cardAmount.HasValue)
        {
            query = query.Where(x => x.CardAmount == getTradingsDto.cardAmount);
        }
        if (getTradingsDto.exchangeRate.HasValue)
        {
            query = query.Where(x => x.ExchangeRate == getTradingsDto.exchangeRate);
        }
        if (getTradingsDto.tradeDateTime.HasValue)
        {
            query = query.Where(x => x.CreatedAt == getTradingsDto.tradeDateTime);
        }
        if (getTradingsDto.tradeType.HasValue)
        {
            query = query.Where(x => x.Type == getTradingsDto.tradeType);
        }
        if (getTradingsDto.validUntil.HasValue)
        {
            query = query.Where(x => x.ValidUntil <= getTradingsDto.validUntil);
        }
        if (!string.IsNullOrWhiteSpace(getTradingsDto.otherDetails))
        {
            var otherDetails = getTradingsDto.otherDetails.ToLower();
            query = query.Where(x => EF.Functions.Like(x.OtherDetails.ToLower(), $"%{otherDetails}%"));
        }
        var tradingsList = await query.Include(x => x.GiftCard)
            .ThenInclude(crd => crd.GiftCardRates)
            .ToListAsync();

        var tradeResponse = tradingsList.Select(trade =>
        {
            var giftCard = trade.GiftCard;
            var giftCardRates = giftCard?.GiftCardRates ?? Enumerable.Empty<GiftCardRate>();

            return new WalletTradingResponse(
                tradeId: trade.Id,
                exchangeValue: trade.ExchangeValue,
                cardImageUrl: trade.CardImageUrl,
                cardAmount: trade.CardAmount,
                exchangeRate: trade.ExchangeRate,
                tradeDateTime: trade.CreatedAt,
                tradeType: trade.Type,
                validUntil: trade.ValidUntil,
                otherDetails: trade.OtherDetails,
                giftCardDetails: new GiftCardResponseDto(
                    giftCard?.Category,
                    giftCard?.SubCategory,
                    giftCardRates.Select(rate => new GiftCardRateResponseDto(
                        rate.CountryCode,
                        rate.Currency,
                        rate.Rate,
                        trade.GiftCardId,
                        giftCardRates.FirstOrDefault()?.Id
                    )).ToList()
                ),
                walletId: trade.WalletId
            );
        }).ToList();

        return StandardResponse<IEnumerable<WalletTradingResponse>>.Success(tradeResponse);
    }

    public async Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetUserTradingsAsync
        (GetTradingsDto getTradingsDto, string userId)
    {
        var query = _tradingRepo.GetNonDeletedByCondition(x=>x.CreatedBy == userId);
        if (!string.IsNullOrWhiteSpace(getTradingsDto.tradeId))
        {
            var tradeId = getTradingsDto.tradeId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.Id.ToLower(), $"{tradeId}"));
        }
        if (!string.IsNullOrWhiteSpace(getTradingsDto.giftCardId))
        {
            var giftCardId = getTradingsDto.giftCardId.ToLower();
            query = query.Where(x => EF.Functions.Like(x.GiftCardId.ToLower(), $"{giftCardId}"));
        }
        if (getTradingsDto.exchangeValue.HasValue)
        {
            query = query.Where(x => x.ExchangeValue == getTradingsDto.exchangeValue);
        }
        if (getTradingsDto.cardAmount.HasValue)
        {
            query = query.Where(x => x.CardAmount == getTradingsDto.cardAmount);
        }
        if (getTradingsDto.exchangeRate.HasValue)
        {
            query = query.Where(x => x.ExchangeRate == getTradingsDto.exchangeRate);
        }
        if (getTradingsDto.tradeDateTime.HasValue)
        {
            query = query.Where(x => x.CreatedAt == getTradingsDto.tradeDateTime);
        }
        if (getTradingsDto.tradeType.HasValue)
        {
            query = query.Where(x => x.Type == getTradingsDto.tradeType);
        }
        if (getTradingsDto.validUntil.HasValue)
        {
            query = query.Where(x => x.ValidUntil <= getTradingsDto.validUntil);
        }
        if (!string.IsNullOrWhiteSpace(getTradingsDto.otherDetails))
        {
            var otherDetails = getTradingsDto.otherDetails.ToLower();
            query = query.Where(x => EF.Functions.Like(x.OtherDetails.ToLower(), $"%{otherDetails}%"));
        }
        var tradingsList = await query.Include(x => x.GiftCard)
            .ThenInclude(crd => crd.GiftCardRates)
            .ToListAsync();

        var tradeResponse = tradingsList.Select(trade =>
        {
            var giftCard = trade.GiftCard;
            var giftCardRates = giftCard?.GiftCardRates ?? Enumerable.Empty<GiftCardRate>();

            return new WalletTradingResponse(
                tradeId: trade.Id,
                exchangeValue: trade.ExchangeValue,
                cardImageUrl: trade.CardImageUrl,
                cardAmount: trade.CardAmount,
                exchangeRate: trade.ExchangeRate,
                tradeDateTime: trade.CreatedAt,
                tradeType: trade.Type,
                validUntil: trade.ValidUntil,
                otherDetails: trade.OtherDetails,
                giftCardDetails: new GiftCardResponseDto(
                    giftCard?.Category,
                    giftCard?.SubCategory,
                    giftCardRates.Select(rate => new GiftCardRateResponseDto(
                        rate.CountryCode,
                        rate.Currency,
                        rate.Rate,
                        trade.GiftCardId,
                        giftCardRates.FirstOrDefault()?.Id
                    )).ToList()
                ),
                walletId: trade.WalletId
            );
        }).ToList();

        return StandardResponse<IEnumerable<WalletTradingResponse>>.Success(tradeResponse);
    }

    public async Task<StandardResponse<WalletTradingResponse>>
        SellGiftCardAsync
        (SellGiftCardDto sellGiftCardDto, string userId)
    {
        var cardToSellResponse = await _giftCardServices.GetGiftCardByIdAsync(sellGiftCardDto.giftCardId);

        if (cardToSellResponse.Succeeded)
        {
            if (cardToSellResponse.Data is not null)
            {
                var walletExists = await _walletRepo.ExistsByCondition(x => x.Id == sellGiftCardDto.walletId);

                if (!walletExists)
                {
                    string? errorMsg = "Invalid wallet account";
                    return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
                }


                var exchangeRate = cardToSellResponse.Data.GiftCardRates
                    .FirstOrDefault(x => x.giftCardRateId == sellGiftCardDto.giftCardRateId)?.rate;
                var trade = new WalletTrading
                {
                    Status = TradingStatus.Pending,
                    Type = TradeType.Buy,
                    ExchangeValue = exchangeRate * sellGiftCardDto.cardAmount,
                    CardAmount = sellGiftCardDto.cardAmount,
                    ExchangeRate = exchangeRate,
                    GiftCardId = sellGiftCardDto.giftCardId,
                    WalletId = sellGiftCardDto.walletId
                };
                if (sellGiftCardDto.cardImage is not null)
                {
                    var imageUploadDetails = await _cloudinaryServices.UploadFileToCloudinaryAsync(sellGiftCardDto.cardImage);
                    if (!imageUploadDetails.Succeeded)
                    {
                        string? errorMsg = "Card image upload failed. Kindly retry";
                        return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
                    }
                    trade.CardImageUrl = imageUploadDetails.Data.fileUrlPath;
                    trade.CardImageId = imageUploadDetails.Data.filePublicId;
                }

                var transaction = new WalletTransaction
                {
                    Amount = sellGiftCardDto.cardAmount,
                    TransactionReference = trade.Id,
                    Type = TransactionType.GiftCardPurchase,
                    WalletId = sellGiftCardDto.walletId,
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
                    giftCardDetails: cardToSellResponse.Data,
                    walletId: trade.WalletId
                );
                return StandardResponse<WalletTradingResponse>.Success(tradeResponse);
            }
        }
        string? errorMessage = "Card currently unavailable. Try again later";
        return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
    }
}
