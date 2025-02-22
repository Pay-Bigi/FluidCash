namespace FluidCash.Models;

public class Account: BaseEntity
{
    public string? DpUrl { get; set; }
    public string? DpCloudinaryId { get; set; }
    public string? DisplayName { get; set; }
    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}
