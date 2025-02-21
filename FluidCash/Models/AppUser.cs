namespace FluidCash.Models;

public class AppUser : BaseEntity
{
    public string? HashedTransactionPin { get; set; }
    public string? AccountId { get; set; }
    public Account? Account { get; set; }
}
