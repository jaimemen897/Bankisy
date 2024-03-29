using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("cards")]
public class Card
{
    [Column("card_number"), Required, StringLength(16), Key] public string CardNumber { get; set; }
    
    [Column("pin"), Required] public string Pin { get; set; }
    
    [Column("card_type"), Required] public CardType CardType { get; set; }
    
    [Column("expiration_date"), Required] public DateTime ExpirationDate { get; set; }
    
    [Column("cvv"), Required] public string Cvv { get; set; }
    
    [Column("is_deleted")] public bool IsDeleted { get; set; }
    
    [Column("is_blocked")] public bool IsBlocked { get; set; }
    
    [Column("user_id"), Required] public Guid UserId { get; set; }
    
    [Column("bank_account_iban"), Required] public string BankAccountIban { get; set; }
    
    public User User { get; set; }
    
    public BankAccount BankAccount { get; set; }
}

public enum CardType
{
    Debit,
    Visa,
    Credit,
    Prepaid,
    Virtual,
    Mastercard,
    AmericanExpress
}