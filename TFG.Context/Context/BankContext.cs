using Microsoft.EntityFrameworkCore;
using TFG.Context.Models;

namespace TFG.Context.Context;

public class BankContext : DbContext
{
    public BankContext(DbContextOptions<BankContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<BankAccount> BankAccounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.BankAccountOrigin)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.IdAccountOrigin)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.BankAccountDestination)
            .WithMany()
            .HasForeignKey(t => t.IdAccountDestination)
            .OnDelete(DeleteBehavior.Restrict);
    }

}