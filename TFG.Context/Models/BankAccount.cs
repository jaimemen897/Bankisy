using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("bank_accounts")]
public class BankAccount
{
    [Column("id"), Key] public Guid Id { get; set; }

    [Column("balance")] public decimal Balance { get; set; }

    [Column("account_type")] public AccountType AccountType { get; set; }
    
    [Column("is_deleted")] public bool IsDeleted { get; set; }

    [Column("user_id")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public User User { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public enum AccountType
{
    Saving,
    Current,
    FixedTerm,
    Payroll,
    Student
}