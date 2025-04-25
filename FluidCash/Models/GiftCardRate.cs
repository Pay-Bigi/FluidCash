namespace FluidCash.Models;

public class GiftCardRate: BaseEntity
{
    public string? CountryCode { get; set; }
    public string? Currency { get; set; }
    public decimal Rate { get; set; }
    public decimal? SellChargeRate { get; set; } = 5.00M;
    public string? GiftCardId { get; set; }
    public GiftCard? GiftCard { get; set; }
}
