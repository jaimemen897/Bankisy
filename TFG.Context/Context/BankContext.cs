using Microsoft.EntityFrameworkCore;
using TFG.Context.Models;

namespace TFG.Context.Context;

public class BankContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<BankAccount> BankAccounts { get; set; }

    public BankContext(DbContextOptions<BankContext> options) : base(options)
    {
    }
}