namespace FluidCash.IExternalServicesRepo;

public interface IRedisCacheService
{
    Task<bool> SetAsync<T>(string key, T value, TimeSpan expiry);
    Task<T?> GetAsync<T>(string key);
    Task<bool> RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
