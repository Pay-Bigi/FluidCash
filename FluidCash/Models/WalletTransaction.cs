using FluidCash.Helpers.Enums;

namespace FluidCash.Models;

public class WalletTransaction
{
    public TransactionType? Type { get; set; }
    public string? TransactionReference { get; set; }
    public decimal Amount { get; set; }
    public string? OtherDetails { get; set; }
    public string? WalletId { get; set; }
    public Wallet? Wallet { get; set; }
    public string? TradingId { get; set; }
    public WalletTrading? Trading { get; set; }
}
