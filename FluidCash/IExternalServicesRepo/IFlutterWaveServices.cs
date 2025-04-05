namespace FluidCash.IExternalServicesRepo;

public interface IFlutterWaveServices
{
    Task<bool> CreateAuthTokenAsync();
}
