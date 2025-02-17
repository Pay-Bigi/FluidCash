namespace FluidCash.Models;

public class AppUser
{
    public string? HashedTransactionPin { get; set; }
    public string? AccountId { get; set; }
    public Account? Account { get; set; }
}
