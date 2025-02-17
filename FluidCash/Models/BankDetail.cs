namespace FluidCash.Models;

public class BankDetail:BaseEntity
{
    public string? BankCode { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountId { get; set; }
    public Account? Account { get; set; }
}