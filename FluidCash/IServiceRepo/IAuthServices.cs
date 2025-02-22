using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IServiceRepo;

public interface IAuthServices
{
    Task<StandardResponse<string>>
        CreateAccountAsync(CreateAccountDto createAccountDto);

    Task<StandardResponse<string>>
    SetTransactionPasswordWithOtpAsync
    (TransactionPasswordParams passwordParams);

    Task<StandardResponse<bool>>
        VerifyTransactionPasswordAsync
        (TransactionPasswordParams passwordParams);
}
