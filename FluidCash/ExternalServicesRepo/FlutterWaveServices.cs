using Azure.Core;
using FluidCash.Helpers.ServiceConfigs.External;
using FluidCash.IExternalServicesRepo;

namespace FluidCash.ExternalServicesRepo;

public sealed class FlutterWaveServices: IFlutterWaveServices
{
    private readonly HttpClient _flutterAuthHttpClient;
    private readonly HttpClient _flutterServicesHttpClient;

    private string? accessToken = null;

    public FlutterWaveServices(IHttpClientFactory httpClientFactory)
    {
        _flutterAuthHttpClient = httpClientFactory.CreateClient("flutterAuth");
        _flutterServicesHttpClient = httpClientFactory.CreateClient("flutterServices");
    }

    public async Task<bool> CreateAuthTokenAsync()
    {
        using (var response = await _flutterAuthHttpClient.PostAsync("", null))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<InterswitchAuthResponse>();
            accessToken = body?.AccessToken;
        }
        return true;
    }
}
