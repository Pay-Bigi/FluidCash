using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IAccountMgtServices
{
    Task<StandardResponse<string>>
       CreateBankDetails
       (BankDetailsDto bankDetailsDto, string userId);

    Task<StandardResponse<IEnumerable<AccountResponseDto>>>
        GetAllAccountsAsync(AccountsFilterParams accountsFilterParams);

    Task<bool> CreateUserAccountAsync
        (CreateUserAccountParams createUserAccountDto);

    Task<StandardResponse<DashboardResponse>>
        GetUserDashboardAsync(string userId);

    Task<StandardResponse<AccountResponseDto>> 
        GetUserAccountsAsync (string accountId);

    Task<StandardResponse<string>>
        UploadDpAsync(UploadDpParams uploadDpParams, string? userId);

    Task<StandardResponse<string>>
        DeleteDpAsync(string accountId, string userId);

    Task<StandardResponse<string>>
        UpdateBankDetails(BankDetailsDto bankDetailsDto, string userId);

    Task<StandardResponse<string>>
        InitiateWithdrawalAsync(InitiateWithdrawalParams withdrawalParams, string userId);

    Task<StandardResponse<string>>
        ConfirmWithdrawalAsync(ConfirmWithdrawalParams confirmWithdrawalParams, string? userId);
}
