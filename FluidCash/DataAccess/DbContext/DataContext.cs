using FluidCash.DataAccess.DataConfigs;
using FluidCash.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.DataAccess.DbContext;

public sealed class DataContext:IdentityDbContext<AppUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔹 One-to-One: AppUser ↔ Account
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Account)
            .WithOne(a => a.AppUser)
            .HasForeignKey<Account>(a => a.AppUserId)
            .OnDelete(DeleteBehavior.Cascade); // Optional, defines delete behavior

        // 🔹 One-to-One: Account ↔ Wallet
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Wallet)
            .WithOne(w => w.Account)
            .HasForeignKey<Wallet>(w => w.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-One: Account ↔ BankDetail
        modelBuilder.Entity<Account>()
            .HasOne(a => a.BankDetail)
            .WithOne(b => b.Account)
            .HasForeignKey<BankDetail>(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-Many: Wallet ↔ WalletTransaction
        modelBuilder.Entity<Wallet>()
            .HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-Many: Wallet ↔ WalletTrading
        modelBuilder.Entity<Wallet>()
            .HasMany(w => w.Tradings)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-Many: GiftCard ↔ GiftCardRate
        modelBuilder.Entity<GiftCard>()
            .HasMany(g => g.GiftCardRates)
            .WithOne(c=>c.GiftCard)
            .HasForeignKey(g=>g.GiftCardId) // Ensures proper FK mapping
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-Many: GiftCard ↔ GiftCardRate
        modelBuilder.Entity<WalletTrading>()
            .HasOne(g => g.GiftCard)
            .WithMany()
            .HasForeignKey(g => g.GiftCardId) // Ensures proper FK mapping
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 One-to-One: WalletTrading ↔ WalletTransaction (optional)
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(t => t.Trading)
            .WithOne(g=>g.Transaction) // If one transaction per trade, use .WithOne()
            .HasForeignKey<WalletTrading>(t => t.TransactionId)
            .OnDelete(DeleteBehavior.SetNull); // Prevent deletion cascade

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
