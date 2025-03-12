using FluidCash.Helpers.ObjectFormatters.DTOs.Requests;

namespace FluidCash.IServiceRepo;

public interface IAccountMgtServices
{
    Task<bool> CreateUserAccountAsync(CreateUserAccountDto createUserAccountDto);
}
