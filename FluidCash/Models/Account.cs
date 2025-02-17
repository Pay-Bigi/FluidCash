namespace FluidCash.Models;

public class Account: BaseEntity
{
    public string? DpUrl { get; set; }
    public string? displayName { get; set; }
    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}
