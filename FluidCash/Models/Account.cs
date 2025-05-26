namespace FluidCash.Models;

public class Account: BaseEntity
{
    public string? DpUrl { get; set; }
    public string? DpCloudinaryId { get; set; }
    public string? DisplayName { get; set; }
    public string? AppUserId { get; set; }
    public bool? IsNotificationEnabled { get; set; } 
    public AppUser? AppUser { get; set; }
    public string? WalleId { get; set; }
    public Wallet? Wallet { get; set; }
    public string? BankDetailId { get; set; }
    public BankDetail? BankDetail { get; set; }
}
