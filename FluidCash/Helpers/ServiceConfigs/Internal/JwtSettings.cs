namespace FluidCash.Helpers.ServiceConfigs.Internal;

public class JwtSettings
{
    public string? TokenKey { get; set; }
    public string? ValidAudience { get; set; }
    public string? ValidIssuer { get; set; }
    public string? Expires { get; set; }
}