using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IExternalServicesRepo;

public interface IInterswitchServices
{
    Task<StandardResponse<string>> RechargeAirtimeAsync(string? phoneNumber, string? amount, string? network);
}
