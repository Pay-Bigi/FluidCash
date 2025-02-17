namespace FluidCash.Models;

public class Wallet:BaseEntity
{
    public string? currency { get; set; }
    public decimal Balance { get; set; }
    public string? AccountId { get; set; }
    public Account? Account { get; set; }
    public virtual ICollection<WalletTransaction>? Transactions { get; set; }
    public virtual ICollection<WalletTrading>? Tradings { get; set; }
}
