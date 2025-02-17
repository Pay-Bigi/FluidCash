namespace FluidCash.Models;

public class WalletTrading: BaseEntity
{
    public string? ExchangeValue { get; set; }
    public string? CardImageUrl { get; set; }
    public decimal CardAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public string? GiftCardId { get; set; }
    public string? WalletId { get; set; }
    public Wallet? Wallet { get; set; }
}
