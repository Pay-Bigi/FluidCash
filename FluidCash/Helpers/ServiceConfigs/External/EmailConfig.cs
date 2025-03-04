namespace FluidCash.Helpers.ServiceConfigs.External;

public class EmailConfig
{
    public string? HOST { get; set; }
    public string? MAILFROM { get; set; }
    public string? USERNAME { get; set; }
    public string? PASSWORD { get; set; }
    public bool ENABLESSL { get; set; } = true;
    public int PORT { get; set; }
}
