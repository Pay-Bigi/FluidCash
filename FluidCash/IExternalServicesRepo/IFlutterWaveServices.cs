using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;

namespace FluidCash.IExternalServicesRepo;

public interface IFlutterWaveServices
{
    Task<StandardResponse<string>> RechargeAirtimeAsync(string? phoneNumber, string? amount, string? network);
}
