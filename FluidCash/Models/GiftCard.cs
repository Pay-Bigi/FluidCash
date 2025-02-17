namespace FluidCash.Models;

public class GiftCard:BaseEntity
{
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public virtual ICollection<GiftCardRate>? GiftCardRates { get; set; }
}
