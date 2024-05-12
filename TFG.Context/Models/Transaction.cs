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

    [Column("concept")]
    [MaxLength(255, ErrorMessage = "Concept must be less than 255 characters")]
    [Required(ErrorMessage = "Concept is required")]
    public string Concept { get; set; }

    [Column("amount")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive number")]
    public decimal Amount { get; set; }

    [Column("iban_account_origin")]
    [ForeignKey("BankAccountOriginIban")]
    [StringLength(34, ErrorMessage = "IBAN of origin account must be 34 characters")]
    public string? IbanAccountOrigin { get; set; }

    [Column("iban_account_destination")]
    [ForeignKey("BankAccountDestinationIban")]
    [StringLength(34, ErrorMessage = "IBAN of destination account must be 34 characters")]
    [Required(ErrorMessage = "IBAN of destination account is required")]
    public string IbanAccountDestination { get; set; }

    [Column("date")]
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; } = DateTime.Now.ToUniversalTime();

    public BankAccount BankAccountOriginIban { get; set; }

    public BankAccount BankAccountDestinationIban { get; set; }
}