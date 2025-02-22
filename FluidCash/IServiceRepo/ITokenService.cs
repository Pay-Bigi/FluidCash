using FluidCash.Models;

namespace FluidCash.IServiceRepo;

public interface ITokenService
{
    Task<string> CreateTokenAsync(AppUser user);
}
