using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FluidCash.ServiceRepo;

public sealed class WalletServices : IWalletServices
{
    private readonly IBaseRepo<Wallet> _walletRepo;

    public WalletServices(IBaseRepo<Wallet> walletRepo)
    {
        _walletRepo = walletRepo;
    }

    public async Task<bool>
        ConfirmWalletExistsAsync(string? walletId)
    {
        bool walletExists = await _walletRepo.ExistsByConditionAsync(wallet => wallet.Id == walletId);
        return walletExists;
    }

    public async Task<StandardResponse<string>>
        CreateWalletAsync(CreateWalletParams createWalletDto, string userId)
    {
        var walletToCreate = new Wallet
        {
            AccountId = createWalletDto.accountId,
            Balance = createWalletDto.balance,
            Currency = createWalletDto.currency,
            CreatedBy = userId,
            CreatedAt = DateTime.Now
        };
        await _walletRepo.AddAsync(walletToCreate);
        string? walletId = walletToCreate.Id;
        return StandardResponse<string>.Success(walletId);
    }

    public async Task<bool>
        CreditWalletAsync(CreditAndDebitWalletParams? creditWalletParams, string? userId)
    {
        var accountToCredit = _walletRepo.GetNonDeletedByCondition(wallet => wallet.Id == creditWalletParams.walletId)
            .FirstOrDefault();
        if (accountToCredit is null)
        {
            return false;
        }
        accountToCredit.Balance += creditWalletParams.amount;
        await _walletRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DebitWalletAsync(CreditAndDebitWalletParams? debitWalletParams, string? userId)
    {
        var accountToCredit = _walletRepo.GetNonDeletedByCondition(wallet => wallet.Id == debitWalletParams.walletId)
            .FirstOrDefault();
        if (accountToCredit is null)
        {
            return false;
        }
        if(accountToCredit.Balance <debitWalletParams.amount) { return false; }
        accountToCredit.Balance -= debitWalletParams.amount;
        await _walletRepo.SaveChangesAsync();
        return true;
    }

    /*public async Task<StandardResponse<IEnumerable<WalletResponseDto>>>
        GetAllWalletsAsync()
    {
        var allWallets = await _walletRepo.GetAllNonDeleted().OrderByDescending(wlt => wlt.CreatedAt).ToListAsync();

        var walletResponse = allWallets.Select(wlt => new WalletResponseDto
            (
                wlt.Currency,
                wlt.Balance,
                wlt.Transactions.Select(wltTran => new WalletTransactionResponse
                (
                    wltTran.Id,
                    wltTran.Type,
                    wltTran.TransactionReference,
                    wltTran.Amount,
                    wltTran.OtherDetails,
                    new WalletTradingResponse
                    (wltTran.Trading.Id, wltTran.Trading.ExchangeValue,
                    wltTran.Trading.CardImageUrl, wltTran.Trading.CardAmount,
                    wltTran.Trading.ExchangeRate, wltTran.Trading.CreatedAt,
                    wltTran.Trading.Type, wltTran.Trading.ValidUntil,
                    wltTran.Trading.OtherDetails,
                     new GiftCardResponseDto(
                    wltTran.Trading.GiftCard.Category,
                    wltTran.Trading.GiftCard.SubCategory,
                    wltTran.Trading.GiftCard.GiftCardRates.Select(rate => new GiftCardRateResponseDto(
                        rate.CountryCode,
                        rate.Currency,
                    rate.Rate,
                        wltTran.Trading.GiftCard.Id,
                        rate.Id
                    )).ToList()
                    ),
                     wltTran.Trading.WalletId
                ))
            )));
        return StandardResponse<IEnumerable<WalletResponseDto>>.Success(walletResponse);
    }

    public async Task<StandardResponse<WalletResponseDto>> GetUserWalletAsync(string? walletId, string? userId)
    {
        var userWallet = await _walletRepo.GetNonDeletedByCondition(wlt=>wlt.Id == walletId && wlt.CreatedBy == userId)
            .OrderByDescending(wlt => wlt.CreatedAt).FirstOrDefaultAsync();

        var walletResponse = new WalletResponseDto
            (
                userWallet.Currency,
                userWallet.Balance,
                userWallet.Transactions.Select(wltTran => new WalletTransactionResponse
                (
                    wltTran.Id,
                    wltTran.Type,
                    wltTran.TransactionReference,
                    wltTran.Amount,
                    wltTran.OtherDetails,
                    new WalletTradingResponse
                    (wltTran.Trading.Id, wltTran.Trading.ExchangeValue,
                    wltTran.Trading.CardImageUrl, wltTran.Trading.CardAmount,
                    wltTran.Trading.ExchangeRate, wltTran.Trading.CreatedAt,
                    wltTran.Trading.Type, wltTran.Trading.ValidUntil,
                    wltTran.Trading.OtherDetails,
                     new GiftCardResponseDto(
                    wltTran.Trading.GiftCard.Category,
                    wltTran.Trading.GiftCard.SubCategory,
                    wltTran.Trading.GiftCard.GiftCardRates.Select(rate => new GiftCardRateResponseDto(
                        rate.CountryCode,
                        rate.Currency,
                    rate.Rate,
                        wltTran.Trading.GiftCard.Id,
                        rate.Id
                    )).ToList()
                    ),
                     wltTran.Trading.WalletId
                ))
            ));
        return StandardResponse<WalletResponseDto>.Success(walletResponse);
    }*/


    public async Task<StandardResponse<IEnumerable<WalletResponseDto>>> GetAllWalletsAsync()
    {
        var allWallets = await _walletRepo
            .GetAllNonDeleted()
            .AsNoTracking()
            .Include(wlt => wlt.Transactions)
                .ThenInclude(tran => tran.Trading)
                    .ThenInclude(trading => trading.GiftCard)
                        .ThenInclude(giftCard => giftCard.GiftCardRates)
            .OrderByDescending(wlt => wlt.CreatedAt)
            .ToListAsync();

        var walletResponses = allWallets.Select(MapToWalletResponseDto);

        return StandardResponse<IEnumerable<WalletResponseDto>>.Success(walletResponses);
    }

    public async Task<StandardResponse<WalletResponseDto>> GetUserWalletAsync(string? walletId, string? userId)
    {
        var userWallet = await _walletRepo
            .GetNonDeletedByCondition(wlt => wlt.Id == walletId && wlt.CreatedBy == userId)
            .AsNoTracking()
            .Include(wlt => wlt.Transactions)
                .ThenInclude(tran => tran.Trading)
                    .ThenInclude(trading => trading.GiftCard)
                        .ThenInclude(giftCard => giftCard.GiftCardRates)
            .OrderByDescending(wlt => wlt.CreatedAt)
            .FirstOrDefaultAsync();

        if (userWallet == null)
            { 
            string? errorMsg = "Wallet not found";
            return StandardResponse<WalletResponseDto>.Failed(data: null, errorMsg); }

        return StandardResponse<WalletResponseDto>.Success(MapToWalletResponseDto(userWallet));
    }

    #region Private Methods
    private WalletResponseDto MapToWalletResponseDto(Wallet wallet)
    {
        return new WalletResponseDto
        (
            wallet.Currency,
            wallet.Balance,
            wallet.Transactions.Select(MapToWalletTransactionResponse).ToList()
        );
    }

    private WalletTransactionResponse MapToWalletTransactionResponse(WalletTransaction transaction)
    {
        return new WalletTransactionResponse
        (
            transaction.Id,
            transaction.Type,
            transaction.TransactionReference,
            transaction.Amount,
            transaction.OtherDetails,
            MapToWalletTradingResponse(transaction.Trading)
        );
    }

    private WalletTradingResponse MapToWalletTradingResponse(WalletTrading trading)
    {
        return new WalletTradingResponse
        (
            trading.Id,
            trading.ExchangeValue,
            trading.CardImageUrl,
            trading.CardAmount,
            trading.ExchangeRate,
            trading.CreatedAt,
            trading.Type,
            trading.ValidUntil,
            trading.OtherDetails,
            new GiftCardResponseDto(
                trading.GiftCard.Category,
                trading.GiftCard.SubCategory,
                trading.GiftCard.GiftCardRates.Select(MapToGiftCardRateResponseDto).ToList()
            ),
            trading.WalletId,
            status: trading.Status
        );
    }

    private GiftCardRateResponseDto MapToGiftCardRateResponseDto(GiftCardRate rate)
    {
        return new GiftCardRateResponseDto(
            rate.CountryCode,
            rate.Currency,
            rate.Rate,
            rate.GiftCard.Id,
            rate.Id
        );
    }
    #endregion
}
