using Azure.Core;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.Helpers.ServiceConfigs.External;
using FluidCash.IExternalServicesRepo;
using System.Net.Http.Headers;

namespace FluidCash.ExternalServicesRepo;

public sealed class InterswitchServices: IInterswitchServices
{
    private readonly HttpClient _interswitchAuthHttpClient;
    private readonly HttpClient _interswitchServicesHttpClient;

    private string? accessToken = string.Empty;
    private string? terminalId = string.Empty;

    public InterswitchServices(IHttpClientFactory httpClientFactory)
    {
        _interswitchAuthHttpClient = httpClientFactory.CreateClient("interswitchAuth");
        _interswitchServicesHttpClient = httpClientFactory.CreateClient("interswitchServices");
    }

    public async Task<StandardResponse<string>>
        RechargeAirtimeAsync(string? phoneNumber, string? amount, string? network)
    { 
        if (string.IsNullOrWhiteSpace(accessToken))  await CreateAuthTokenAsync();
        _interswitchServicesHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var flutterResponse = await _interswitchServicesHttpClient.GetAsync("/services");
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
        using (var response = await _interswitchAuthHttpClient.PostAsync("", null))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<InterswitchAuthResponse>();
            accessToken = body?.AccessToken;
            terminalId = body?.TerminalId;
        }
        return true;
    }
}
