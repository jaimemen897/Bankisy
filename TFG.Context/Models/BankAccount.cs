using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("bank_accounts")]
public class BankAccount
{
    [Column("iban"), Required, StringLength(34), Key] public string Iban { get; set; }

    [Column("balance")] public decimal Balance { get; set; } = 0;

    [Column("account_type")] public AccountType AccountType { get; set; }
    
    [Column("is_deleted")] public bool IsDeleted { get; set; }

    public List<User> UsersId { get; set; } = [];
    
    public ICollection<Transaction> TransactionsOrigin { get; set; } = new List<Transaction>();

    public ICollection<Transaction> TransactionsDestination { get; set; } = new List<Transaction>();
    
    public List<Card> Cards { get; set; } = [];
}

public enum AccountType
{
    Saving,
    Current,
    FixedTerm,
    Payroll,
    Student
}