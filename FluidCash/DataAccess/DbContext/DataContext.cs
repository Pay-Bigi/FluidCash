using FluidCash.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.DataAccess.DbContext;

public class DataContext:IdentityDbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<GiftCardRate> GiftCardRates { get; set; }
    public DbSet<WalletTrading> WalletTradings { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<BankDetail> BankDetails { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
}
