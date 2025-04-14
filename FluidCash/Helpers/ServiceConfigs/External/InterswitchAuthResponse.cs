using System.Text.Json.Serialization;

namespace FluidCash.Helpers.ServiceConfigs.External;

public class InterswitchAuthResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("merchant_code")]
    public string? MerchantCode { get; set; }

    [JsonPropertyName("client_authorization_domain")]
    public string? ClientAuthorizationDomain { get; set; }

    [JsonPropertyName("requestor_id")]
    public string? RequestorId { get; set; }

    [JsonPropertyName("api_resources")]
    public List<string>? ApiResources { get; set; }

    [JsonPropertyName("merchant-wallet-actions")]
    public List<string>? MerchantWalletActions { get; set; }

    [JsonPropertyName("incognito_requestor_id")]
    public string? IncognitoRequestorId { get; set; }

    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("client_logo")]
    public string? ClientLogo { get; set; }

    [JsonPropertyName("payable_id")]
    public string? PayableId { get; set; }

    [JsonPropertyName("client_description")]
    public string? ClientDescription { get; set; }

    [JsonPropertyName("jti")]
    public string? Jti { get; set; }

    [JsonPropertyName("terminalId")]
    public string? TerminalId { get; set; }
}