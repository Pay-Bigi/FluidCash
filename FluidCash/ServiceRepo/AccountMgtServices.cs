using FluidCash.DataAccess.Repo;
using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using FluidCash.IServiceRepo;
using FluidCash.Models;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.ServiceRepo;

public sealed class AccountMgtServices : IAccountMgtServices
{
    private readonly IBaseRepo<Account> _accountRepo;
    private readonly IWalletServices _walletServices;
    private readonly ICloudinaryServices _cloudinaryServices;
    private readonly IGiftCardServices _giftCardServices;
    private readonly IPaystackServices _paystackServices;
    private readonly IBaseRepo<BankDetail> _bankDetailRepo;

    public AccountMgtServices(IBaseRepo<Account> accountRepo, IWalletServices walletServices,
        ICloudinaryServices cloudinaryServices, IGiftCardServices giftCardServices,
        IPaystackServices paystackServices, IBaseRepo<BankDetail> bankDetailRepo)
    {
        _accountRepo = accountRepo;
        _walletServices = walletServices;
        _cloudinaryServices = cloudinaryServices;
        _giftCardServices = giftCardServices;
        _paystackServices = paystackServices;
        _bankDetailRepo = bankDetailRepo;
    }


    public async Task<bool> 
        CreateUserAccountAsync
        (CreateUserAccountParams createUserAccountDto)
    {
        try
        {
            var userAccount = new Account
            {
                DisplayName = createUserAccountDto.displayName,
                AppUserId = createUserAccountDto.appUserId,
                DpCloudinaryId = createUserAccountDto.dpCloudinaryId,
                DpUrl = createUserAccountDto.dpUrl,
                CreatedAt = DateTime.Now,
                CreatedBy = createUserAccountDto.appUserId
            };

            // Create Wallet
            var walletCreationParams = new CreateWalletParams("NGN", 0, userAccount.Id);
            var walletId = await _walletServices.CreateWalletAsync(walletCreationParams, userAccount.AppUserId);

            userAccount.WalleId = walletId.Data;
            await _accountRepo.AddAsync(userAccount);
            await _accountRepo.SaveChangesAsync();

            return true;
        }
        catch (Exception ex) 
        {
            return false;
        }
    }

    public async Task<StandardResponse<string>>
        DeleteDpAsync
        (string accountId, string userId)
    {
        var account = _accountRepo.GetNonDeletedByCondition(x => x.Id == accountId)
            .FirstOrDefault();
        if (account is null)
        {
            string? errorMessage = "Account not found";
            return StandardResponse<string>.Failed(data: null, errorMessage);
        }
        if (string.IsNullOrWhiteSpace(account.DpCloudinaryId))
        {
            string? errorMessage = "No existing display image";
            return StandardResponse<string>.Failed(data: null, errorMessage);
        }
        var resp = await _cloudinaryServices.DeleteFileFromCloudinaryAsync(account.DpCloudinaryId);
        if (!resp.Succeeded)
        {
            string? errorMsg = "Failed to delete dp";
            return StandardResponse<string>.Failed(data: null, errorMsg);
        }
        account.DpCloudinaryId = string.Empty;
        account.DpUrl = string.Empty;
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        string? successMsg = "Dp deleted successfully";
        return StandardResponse<string>.Success(data: null, successMessage: successMsg);
    }

    public async Task<StandardResponse<AccountResponseDto>>
        GetUserAccountAsync
        (string accountId)
    {
        var account = await _accountRepo.GetNonDeletedByCondition(acc => acc.Id == accountId)
            .Include(acc => acc.BankDetail)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (account is null)
        {
            string? errorMsg = "Account not found";
            return StandardResponse<AccountResponseDto>.Failed(data: null, errorMessage: errorMsg);
        }
        var accountDto = new AccountResponseDto
            (
                account.Id,
                account.AppUser?.Email,
                account.DisplayName,
                account.DpUrl,
                new BankDetailsResponse
                (
                    account.BankDetail?.BankCode,
                    account.BankDetail?.AccountName,
                    account.BankDetail?.AccountNumber
                )
            );
        return StandardResponse<AccountResponseDto>.Success(data: accountDto);
    }

    public async Task<StandardResponse<IEnumerable<AccountResponseDto>>>
        GetAllAccountsAsync(AccountsFilterParams accountsFilterParams)
    {
        //The query and filter will fail due to Select included in base query
        var accountQuery = _accountRepo.GetAll()
            .Include(x => x.BankDetail)
            .Select(holder => new
            {
                id = holder.Id,
                userMail = holder.AppUser.Email,
                displayName = holder.DisplayName,
                dpUrl = holder.DpUrl,
                bankCode = holder.BankDetail.BankCode,
                accName = holder.BankDetail.AccountName,
                accNumber = holder.BankDetail.AccountNumber

            })
            .AsNoTracking();
        if (!string.IsNullOrEmpty(accountsFilterParams.accountId))
        {
            accountQuery = accountQuery.Where(x => EF.Functions.Like(x.id, accountsFilterParams.accountId));
        }
        if (!string.IsNullOrEmpty(accountsFilterParams.displayName))
        {
            string frmtddisplayName = accountsFilterParams.displayName.ToLower();
            accountQuery = accountQuery.Where(x => EF.Functions.Like(x.displayName.ToLower(), $"%{frmtddisplayName}%"));
        }
        if (!string.IsNullOrEmpty(accountsFilterParams.userMail))
        {
            string frmtduserMail = accountsFilterParams.userMail.ToLower();
            accountQuery = accountQuery.Where(x => EF.Functions.Like(x.userMail.ToLower(), $"%{frmtduserMail}%"));
        }

        var response = await accountQuery.Select(query => new AccountResponseDto
        (
            query.id,
            query.userMail,
            query.displayName,
            query.dpUrl,
            new BankDetailsResponse
            (
                query.bankCode,
                query.accName,
                query.accNumber
            )
        )).ToListAsync();

        return StandardResponse<IEnumerable<AccountResponseDto>>.Success(data: response);
    }

    public async Task<StandardResponse<DashboardResponse>>
        GetUserDashboardAsync(string userId)
    {
        // Fetch account with includes asynchronously
        var account = await _accountRepo
            .GetNonDeletedByCondition(x => x.AppUserId == userId)
            .Include(x => x.AppUser)
            .Include(x => x.Wallet)
                .ThenInclude(wallet => wallet.Transactions)
                .ThenInclude(trans => trans.Trading)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        // If account is not found, return failure response
        if (account is null)
        {
            return StandardResponse<DashboardResponse>.Failed(null, "Account not found");
        }

        // Get wallet transactions for today
        var walletTransactions = account.Wallet?.Transactions
            .Where(trans => trans.CreatedAt >= DateTime.UtcNow.Date)
            .ToList() ?? new List<WalletTransaction>();

        // Fetch gift card details in parallel
        var walletTransactionResponses = await Task.WhenAll(walletTransactions.Select(async trans =>
            new WalletTransactionResponse(
                trans.Id,
                trans.Type,
                trans.TransactionReference,
                trans.Amount,
                trans.OtherDetails,
                trans.Trading != null
                    ? new WalletTradingResponse(
                        trans.Trading.Id,
                        trans.Trading.ExchangeValue,
                        trans.Trading.CardsImageUrl,
                        trans.Trading.CardAmount,
                        trans.Trading.ExchangeRate,
                        trans.Trading.CreatedAt,
                        trans.Trading.Type,
                        trans.Trading.ValidUntil,
                        trans.Trading.OtherDetails,
                        await _giftCardServices.GetGiftCardByIdAsync(trans.Trading.GiftCardId), // Awaiting properly
                        trans.Trading.WalletId,
                        trans.Trading.Status
                    )
                    : null // Handle cases where Trading is null
            )
        ));

        // Construct dashboard response
        var dashBoard = new DashboardResponse(
            account.Id,
            account.AppUser?.Email,
            account.DisplayName,
            account.DpUrl,
            account.Wallet?.Id,
            account.Wallet?.Currency,
            account.Wallet?.Balance,
            walletTransactionResponses.ToList()
        );

        return StandardResponse<DashboardResponse>.Success(dashBoard);
    }

    //Correction needed
    public async Task<StandardResponse<string>>
        InitiateWithdrawalAsync
        (InitiateWithdrawalParams withdrawalParams, string userId)
    {
        var account = _accountRepo.GetNonDeletedByCondition(x => x.Id == withdrawalParams.accountId)
            .Include(acc => acc.AppUser)
            .Include(acc => acc.Wallet)
            .FirstOrDefault();
        if (account is null)
        {
            string? errorMessage = "Account not found";
            return StandardResponse<string>.Failed(data: null, errorMessage);
        }

        bool? is2FaEnabled = account?.AppUser?.TwoFactorEnabled;
        if (is2FaEnabled is true)
        {
            //Create and Send Otp
            throw new NotImplementedException();
        }
        var withdrawalPayload = new InitiateTransferParams
            (
                amount: withdrawalParams.amount,
                accountName: account?.BankDetail?.AccountName,
                accountNumber: account?.BankDetail?.AccountNumber,
                bankCode: account?.BankDetail?.BankCode
            );
        var withdrawalResponse = _paystackServices.InitiateTransfer(withdrawalPayload);
        if (!withdrawalResponse.Succeeded)
        {
            string? errorMsg = "Failed to complete withdrawal";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        account!.Wallet!.Balance -= withdrawalParams.amount;

        //Create withdrawal transaction
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        //Ensure wallet is debited else update wallet
        string? successMsg = "Withdrawal successful";
        return StandardResponse<string>.Success(data: successMsg);
    }

    //Pending on 2FA implementation
    public Task<StandardResponse<string>>
        ConfirmWithdrawalAsync
        (ConfirmWithdrawalParams confirmWithdrawalParams, string? userId)
    {
        throw new NotImplementedException();
    }

    public async Task<StandardResponse<string>>
        UpdateBankDetails
        (BankDetailsDto bankDetailsDto, string userId)
    {
        var account = _accountRepo.GetNonDeletedByCondition(x => x.Id == bankDetailsDto.accountId)
            .Include(acc => acc.BankDetail)
            .FirstOrDefault();
        if (account is null)
        {
            string? errorMsg = "Account not found";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        if (!string.IsNullOrWhiteSpace(bankDetailsDto.accountNumber))
        {
            account.BankDetail.AccountNumber = bankDetailsDto.accountNumber;
        }
        if (!string.IsNullOrWhiteSpace(bankDetailsDto.accountName))
        {
            account.BankDetail.AccountName = bankDetailsDto.accountName;
        }
        if (!string.IsNullOrWhiteSpace(bankDetailsDto.bankCode))
        {
            account.BankDetail.BankCode = bankDetailsDto.bankCode;
        }
        await _accountRepo.SaveChangesAsync();
        string? successMsg = "Bank details updated successfully";
        return StandardResponse<string>.Success(data: null, successMessage: successMsg);
    }

    public async Task<StandardResponse<string>>
       CreateBankDetails
       (BankDetailsDto bankDetailsDto, string userId)
    {
        var account = _accountRepo.GetNonDeletedByCondition(x => x.Id == bankDetailsDto.accountId)
            .FirstOrDefault();
        if (account is null)
        {
            string? errorMsg = "Account not found";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        if (!string.IsNullOrWhiteSpace(account.BankDetailId))
        {
            string errorMsg = "Bank details already exist";
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var bankDetail = new BankDetail
        {
            BankCode = bankDetailsDto.bankCode,
            AccountName = bankDetailsDto.accountName,
            AccountNumber = bankDetailsDto.accountNumber,
            AccountId = bankDetailsDto.accountId,
            CreatedAt = DateTime.Now,
            CreatedBy = userId
        };
        await _bankDetailRepo.AddAsync(bankDetail);
        await _accountRepo.SaveChangesAsync();
        string? successMsg = "Bank details updated successfully";
        return StandardResponse<string>.Success(data: null, successMessage: successMsg);
    }

    public async Task<StandardResponse<string>>
        UploadDpAsync
        (UploadDpParams uploadDpParams, string? userId)
    {
        var account = _accountRepo.GetNonDeletedByCondition(x => x.Id == uploadDpParams.accountId)
            .FirstOrDefault();
        if (account is null)
        {
            string? errorMessage = "Account not found";
            return StandardResponse<string>.Failed(data: null, errorMessage);
        }
        if(!string.IsNullOrWhiteSpace(account.DpCloudinaryId))
        {
            await _cloudinaryServices.DeleteFileFromCloudinaryAsync(account.DpCloudinaryId);
        }
        var uploadResp = await _cloudinaryServices.UploadFileToCloudinaryAsync(uploadDpParams.dp);
        if (!uploadResp.Succeeded)
        {
            string? errorMsg = "Failed to upload dp";
            return StandardResponse<string>.Failed(data: null, errorMsg);
        }
        account.DpCloudinaryId = uploadResp.Data.filePublicId;
        account.DpUrl = uploadResp.Data.fileUrlPath;
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        string? successMsg = "Photo upload successful";
        return StandardResponse<string>.Success(data: null, successMessage: successMsg);
    }
}
