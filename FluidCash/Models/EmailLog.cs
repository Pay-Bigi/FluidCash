namespace FluidCash.Models;

public class EmailLog : BaseEntity
{
    public string? CallingEndpoint { get; set; }
    public string? RecipientEmail { get; set; }
    public string? Detail { get; set; }
    public string? SenderName { get; set; }
    public bool IsSuccessful { get; set; }
}

