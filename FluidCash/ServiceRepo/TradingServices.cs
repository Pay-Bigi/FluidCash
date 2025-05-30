﻿using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.Enums;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FluidCash.ServiceRepo;

public sealed class TradingServices : ITradingServices
{
    private readonly IBaseRepo<WalletTrading> _tradingRepo;
    private readonly IWalletServices _walletServices;
    private readonly IBaseRepo<WalletTransaction> _transactionRepo;
    private readonly IGiftCardServices _giftCardServices;
    private readonly ICloudinaryServices _cloudinaryServices;
    private readonly IPaystackServices _paystackServices;

    public TradingServices(IBaseRepo<WalletTrading> tradingRepo, IWalletServices walletServices,
        IBaseRepo<WalletTransaction> transactionRepo, IGiftCardServices giftCardServices,
        ICloudinaryServices cloudinaryServices, IPaystackServices paystackServices)
    {
        _tradingRepo = tradingRepo;
        _walletServices = walletServices;
        _transactionRepo = transactionRepo;
        _giftCardServices = giftCardServices;
        _cloudinaryServices = cloudinaryServices;
        _paystackServices = paystackServices;
    }

    //Correction Included
    public async Task<StandardResponse<string>>
        ApproveGiftCardSellAsync
        (ApproveGiftCardSellParams approveGiftCardDto, string userId)
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
        if (approveGiftCardDto.isApproved)
        {
            trade.Status = TradingStatus.Approved;
            var creditPayload = new CreditAndDebitWalletParams(trade.WalletId, trade.ExchangeValue);
            var creditWltSuccess = await _walletServices.CreditWalletAsync(creditPayload, userId);
            if (!creditWltSuccess)
            {
                string? errorMsg = "Credit wallet failed. Kindly retry";
                return StandardResponse<string>.Failed(null, errorMsg);
            }
        }
        else
        {
            trade.Status = TradingStatus.Declined;
        }

        //Update Transaction Status

        trade.UpdatedAt = DateTime.UtcNow;
        trade.UpdatedBy = userId;
        _tradingRepo.Update(trade);
        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade status updated successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    //Correction Included
    public async Task<StandardResponse<string>>
        ApproveGiftCardPurchaseAsync
        (ApproveGiftCardPurchaseParams approveGiftCardPurchaseDto, string? userId)
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
            //Update Transaction Status 

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
            var creditPayload = new CreditAndDebitWalletParams(trade.WalletId, trade.CardAmount);
            var creditWltSuccess = await _walletServices.CreditWalletAsync(creditPayload, userId);
            if (!creditWltSuccess)
            {
                string? errorMsg = "Decline failed due to refund error. Kindly retry";
                return StandardResponse<string>.Failed(null, errorMsg);
            }
        }
        trade.UpdatedAt = DateTime.UtcNow;
        trade.UpdatedBy = userId;
        _tradingRepo.Update(trade);
        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade status updated successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    public async Task<StandardResponse<WalletTradingResponse>>
        BuyGiftCardAsync
        (BuyGiftCardParams buyGiftCardDto, string userId)
    {
        var cardToBuyResponse = await _giftCardServices.GetGiftCardByIdAsync(buyGiftCardDto.giftCardId);

        if (cardToBuyResponse is not null)
        {
            var walletExists = await _walletServices.ConfirmWalletExistsAsync(buyGiftCardDto.walletId);

            if (!walletExists)
            {
                string? errorMsg = "Invalid wallet account";
                return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
            }

            var exchangeRate = cardToBuyResponse.GiftCardRates
                .FirstOrDefault(x => x.giftCardRateId == buyGiftCardDto.giftCardRateId)?.rate;
            var exchangeValue = exchangeRate * buyGiftCardDto.amount;

            if (buyGiftCardDto.payFromWallet)
            {
                var debitPayload = new CreditAndDebitWalletParams(buyGiftCardDto.walletId, buyGiftCardDto.amount);
                var debitWalletSucceded = await _walletServices.DebitWalletAsync(debitPayload, userId);
                if (!debitWalletSucceded)
                {
                    string? errorMsg = "Insufficient Balance";
                    return StandardResponse<WalletTradingResponse>.Failed(data: null, errorMsg);
                }
            }
            else
            {
                if (buyGiftCardDto.isPaymentMade)
                {
                    var confirmPaymentResponse = _paystackServices.VerifyTransaction(buyGiftCardDto.paystackTransactionRef);
                    if (!confirmPaymentResponse.Succeeded)
                    {
                        string? errorMsg = "Payment verification failed. Kindly retry or contact your service provider";
                        return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
                    }
                }
                else
                {
                    var transactionInitiationParams = new InitializePaymentParams(buyGiftCardDto.transactionEmail, exchangeValue);
                    var initiateTransactionResponse = _paystackServices.InitiateTransaction(transactionInitiationParams);
                    if (initiateTransactionResponse.Succeeded)
                    {
                        var dataToReturn = initiateTransactionResponse.Data;
                        string paymentPayLoad = JsonSerializer.Serialize(dataToReturn);
                        return StandardResponse<WalletTradingResponse>.Pending(data: null, message: paymentPayLoad);
                    }
                    var errorMsg = initiateTransactionResponse.Message;
                    return StandardResponse<WalletTradingResponse>.Failed(data: null, errorMsg);
                }
            }
            var trade = new WalletTrading
            {
                Status = TradingStatus.Pending,
                Type = TradeType.Buy,
                ExchangeValue = exchangeValue,
                CardAmount = buyGiftCardDto.amount,
                ExchangeRate = exchangeRate,
                GiftCardId = buyGiftCardDto.giftCardId,
                WalletId = buyGiftCardDto.walletId,
                CreatedBy = userId
            };
            var transaction = new WalletTransaction
            {
                Amount = buyGiftCardDto.amount,
                TransactionReference = trade.Id,
                Type = TransactionType.GiftCardPurchase,
                WalletId = buyGiftCardDto.walletId,
                TradingId = trade.Id,
                CreatedBy = userId
            };
            await _tradingRepo.AddAsync(trade);
            await _transactionRepo.AddAsync(transaction);
            await _tradingRepo.SaveChangesAsync();

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
                giftCardDetails: cardToBuyResponse,
                walletId: trade.WalletId,
                status: trade.Status
            );
            return StandardResponse<WalletTradingResponse>.Success(tradeResponse);
        }
        string? errorMessage = "Card currently unavailable. Try again later";
        return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
    }

    public async Task<StandardResponse<string>>
        DeleteTradeAync
        (string tradeId, string userId)
    {
        var tradeToDelete = _tradingRepo.GetNonDeletedByCondition(x => x.Id == tradeId && x.CreatedBy == userId)
            .FirstOrDefault();
        if (tradeToDelete is null)
        {
            string? errorMessage = "Trade not found";
            return StandardResponse<string>.Failed(null, errorMessage);
        }
        _tradingRepo.SoftDelete(tradeToDelete);
        tradeToDelete.UpdatedBy = userId;
        tradeToDelete.UpdatedAt = DateTime.UtcNow;
        _tradingRepo.Update(tradeToDelete);
        await _tradingRepo.SaveChangesAsync();

        string? successMessage = "Trade deleted successfully";
        return StandardResponse<string>.Success(successMessage);
    }

    public async Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetAllTradingsAsync
        (GetTradingsFilterParams getTradingsDto)
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
                        rate.Id
                    )).ToList()
                ),
                walletId: trade.WalletId,
                status: trade.Status
            );
        }).ToList();

        return StandardResponse<IEnumerable<WalletTradingResponse>>.Success(tradeResponse);
    }

    public async Task<StandardResponse<IEnumerable<WalletTradingResponse>>>
        GetUserTradingsAsync
        (GetTradingsFilterParams getTradingsDto, string userId)
    {
        var query = _tradingRepo.GetNonDeletedByCondition(x => x.CreatedBy == userId);
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
                        rate.Id
                    )).ToList()
                ),
                walletId: trade.WalletId,
                status: trade.Status
            );
        }).ToList();

        return StandardResponse<IEnumerable<WalletTradingResponse>>.Success(tradeResponse);
    }

    public async Task<StandardResponse<WalletTradingResponse>>
        SellGiftCardAsync
        (SellGiftCardParams sellGiftCardDto, string userId)
    {
        var cardToSellResponse = await _giftCardServices.GetGiftCardByIdAsync(sellGiftCardDto.giftCardId);

        if (cardToSellResponse is not null)
        {
            var walletExists = await _walletServices.ConfirmWalletExistsAsync(sellGiftCardDto.walletId);

            if (!walletExists)
            {
                string? errorMsg = "Invalid wallet account";
                return StandardResponse<WalletTradingResponse>.Failed(null, errorMsg);
            }


            var exchangeRate = cardToSellResponse.GiftCardRates
                .FirstOrDefault(x => x.giftCardRateId == sellGiftCardDto.giftCardRateId)?.rate;
            var trade = new WalletTrading
            {
                Status = TradingStatus.Pending,
                Type = TradeType.Buy,
                ExchangeValue = exchangeRate * sellGiftCardDto.cardAmount,
                CardAmount = sellGiftCardDto.cardAmount,
                ExchangeRate = exchangeRate,
                GiftCardId = sellGiftCardDto.giftCardId,
                WalletId = sellGiftCardDto.walletId,
                CreatedBy = userId
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
                Type = TransactionType.GiftCardSale,
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
                giftCardDetails: cardToSellResponse,
                walletId: trade.WalletId,
                status: trade.Status
            );
            return StandardResponse<WalletTradingResponse>.Success(tradeResponse);
        }
        string? errorMessage = "Card currently unavailable. Try again later";
        return StandardResponse<WalletTradingResponse>.Failed(null, errorMessage);
    }
}
