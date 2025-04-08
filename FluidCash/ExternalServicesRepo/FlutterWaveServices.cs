using Azure.Core;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.Helpers.ServiceConfigs.External;
using FluidCash.IExternalServicesRepo;
using System.Net.Http.Headers;

namespace FluidCash.ExternalServicesRepo;

public sealed class FlutterWaveServices: IFlutterWaveServices
{
    private readonly HttpClient _flutterAuthHttpClient;
    private readonly HttpClient _flutterServicesHttpClient;

    private string? accessToken = string.Empty;
    private string? terminalId = string.Empty;

    public FlutterWaveServices(IHttpClientFactory httpClientFactory)
    {
        _flutterAuthHttpClient = httpClientFactory.CreateClient("flutterAuth");
        _flutterServicesHttpClient = httpClientFactory.CreateClient("flutterServices");
    }

    public async Task<StandardResponse<string>>
        RechargeAirtimeAsync(string? phoneNumber, string? amount, string? network)
    { 
        if (string.IsNullOrWhiteSpace(accessToken))  await CreateAuthTokenAsync();
        _flutterServicesHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var flutterResponse = await _flutterServicesHttpClient.GetAsync("/services");
        //var content = await flutterResponse.Content.ReadAsStringAsync();

        //if (!flutterResponse.IsSuccessStatusCode)
        //{
        //    throw new Exception($"FlutterWave API failed with status {flutterResponse.StatusCode}, response: {content}");
        //}
        string? successMsg = string.Empty;
        return StandardResponse<string>.Success(successMsg, statusCode:201);
    }

    private async Task<bool> CreateAuthTokenAsync()
    {
        using (var response = await _flutterAuthHttpClient.PostAsync("", null))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<InterswitchAuthResponse>();
            accessToken = body?.AccessToken;
            terminalId = body?.TerminalId;
        }
        return true;
    }
}
