using FluidCash.DataAccess.DataConfigs;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FluidCash.DataAccess.DbContext;

public sealed class DataContext:IdentityDbContext<AppUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Applies UTC to WAT conversion to all DateTime properties
        modelBuilder.ApplyUtcToWatConversion();
        modelBuilder.ApplyConfiguration(new DefaultEmailTemplateSeeding());
        modelBuilder.ApplyConfiguration(new AppRoleSeeding());
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
