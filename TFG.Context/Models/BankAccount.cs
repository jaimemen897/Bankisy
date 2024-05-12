using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("bank_accounts")]
public class BankAccount
{
    [Key]
    [Column("iban")]
    [Required]
    [StringLength(34, ErrorMessage = "IBAN must be 34 characters")]
    public string Iban { get; set; }

    [Column("balance")]
    [Range(0, double.MaxValue, ErrorMessage = "Balance must be a positive number")]
    public decimal Balance { get; set; } = 0;

    [Column("account_type")]
    [Required(ErrorMessage = "Account type is required")]
    public AccountType AccountType { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("accept_bizum")]
    public bool AcceptBizum { get; set; }

    public List<User> Users { get; set; } = [];

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