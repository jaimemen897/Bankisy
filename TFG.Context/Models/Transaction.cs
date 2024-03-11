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
    [MaxLength(255)]
    public string Concept { get; set; }
    
    [Column("amount")]
    public decimal Amount { get; set; }
    
    [Column("id_account_origin")]
    [ForeignKey(nameof(BankAccountOrigin))]
    public Guid IdAccountOrigin { get; set; }
    
    [Column("id_account_destination")]
    [ForeignKey(nameof(BankAccountDestination))]
    public Guid IdAccountDestination { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; } = DateTime.Now.ToUniversalTime();
    
    public BankAccount BankAccountOrigin { get; set; }
    
    public BankAccount BankAccountDestination { get; set; }
}