using FluidCash.Helpers.Enums;

namespace FluidCash.Models;

public class WalletTrading: BaseEntity
{
    public TradingStatus? Status { get; set; }
    public TradeType? Type { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? OtherDetails { get; set; }
    public decimal? ExchangeValue { get; set; }
    public string? CardImageUrl { get; set; }
    public string? CardImageId { get; set; }
    public decimal? CardAmount { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? GiftCardId { get; set; }
    public GiftCard? GiftCard { get; set; }
    public string? WalletId { get; set; }
    public Wallet? Wallet { get; set; }
}
