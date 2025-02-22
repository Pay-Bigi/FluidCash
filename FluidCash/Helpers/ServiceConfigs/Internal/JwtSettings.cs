namespace FluidCash.Helpers.ServiceConfigs.Internal;

public class JwtSettings
{
    public string? TokenKey { get; set; }
    public string? validAudience { get; set; }
    public string? validIssuer { get; set; }
    public string? Expires { get; set; }
}