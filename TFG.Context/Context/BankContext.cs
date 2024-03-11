﻿using Microsoft.EntityFrameworkCore;
using TFG.Context.Models;

namespace TFG.Context.Context;

public class BankContext(DbContextOptions<BankContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    public DbSet<BankAccount> BankAccounts { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.BankAccounts)
            .WithMany(u => u.UsersId)
            .UsingEntity("UserBankAccount",
                l => l.HasOne(typeof(BankAccount)).WithMany().HasForeignKey("BankAccountsId").HasPrincipalKey(nameof(BankAccount.Id)),
                r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UsersId").HasPrincipalKey(nameof(User.Id)),
                j => j.HasKey("UsersId", "BankAccountsId"));


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