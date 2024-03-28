using Microsoft.EntityFrameworkCore;
using TFG.Context.Models;

namespace TFG.Context.Context;

public class BankContext(DbContextOptions<BankContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    public DbSet<BankAccount> BankAccounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }
    
    public DbSet<Card> Cards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.BankAccounts)
            .WithMany(u => u.UsersId)
            .UsingEntity("UserBankAccount",
                l => l.HasOne(typeof(BankAccount)).WithMany().HasForeignKey("BankAccountsId").HasPrincipalKey(nameof(BankAccount.Iban)),
                r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UsersId").HasPrincipalKey(nameof(User.Id)),
                j => j.HasKey("UsersId", "BankAccountsId"));


        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.BankAccountOriginIban)
            .WithMany(b => b.TransactionsOrigin)
            .HasForeignKey(t => t.IbanAccountOrigin);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.BankAccountDestinationIban)
            .WithMany(b => b.TransactionsDestination)
            .HasForeignKey(t => t.IbanAccountDestination);
        
        modelBuilder.Entity<Card>()
            .HasOne(c => c.User)
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId);
        
        modelBuilder.Entity<Card>()
            .HasOne(c => c.BankAccount)
            .WithMany(b => b.Cards)
            .HasForeignKey(c => c.BankAccountIban);
    }
}