using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("transactions")]
public class Transaction
{
    [Column("id")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("concept")] [MaxLength(255)] public string Concept { get; set; }

    [Column("amount")] public decimal Amount { get; set; }

    [Column("iban_account_origin")]
    [ForeignKey("BankAccountOriginIban")]
    public string? IbanAccountOrigin { get; set; }

    [Column("iban_account_destination")]
    [ForeignKey("BankAccountDestinationIban")]
    public string IbanAccountDestination { get; set; }

    [Column("date")] public DateTime Date { get; set; } = DateTime.Now.ToUniversalTime();

    public BankAccount BankAccountOriginIban { get; set; }

    public BankAccount BankAccountDestinationIban { get; set; }
}