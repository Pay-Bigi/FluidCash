using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IWalletServices
{
    Task<StandardResponse<string>>
        CreateWalletAsync(CreateWalletParams createWalletDto, string userId);

    Task<bool> CreditWalletAsync(CreditAndDebitWalletParams? creditWalletParams, string? userId);

    Task<bool> DebitWalletAsync(CreditAndDebitWalletParams? debitWalletParams, string? userId);

    Task<bool> ConfirmWalletExistsAsync(string? walletId);

    Task<StandardResponse<WalletResponseDto>>
        GetUserWalletAsync(string? walletId, string? userId);

    Task<StandardResponse<IEnumerable<WalletResponseDto>>>
        GetAllWalletsAsync();
}
