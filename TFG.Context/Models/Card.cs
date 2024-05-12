using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("cards")]
public class Card
{
    [Key]
    [Column("card_number")]
    [Required(ErrorMessage = "Card number is required")]
    [StringLength(16, ErrorMessage = "Card number must be 16 characters")]
    public string CardNumber { get; set; }

    [Column("pin")]
    [StringLength(4, ErrorMessage = "PIN must be 4 characters")]
    [Required(ErrorMessage = "PIN is required")]
    public string Pin { get; set; }

    [Column("card_type")]
    [Required(ErrorMessage = "Card type is required")]
    public CardType CardType { get; set; }

    [Column("expiration_date")]
    [Required(ErrorMessage = "Expiration date is required")]
    public DateTime ExpirationDate { get; set; }

    [Column("cvv")]
    [StringLength(3, ErrorMessage = "CVV must be 3 characters")]
    [Required(ErrorMessage = "CVV is required")]
    public string Cvv { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("is_blocked")]
    public bool IsBlocked { get; set; }

    [Column("user_id")]
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    [Column("bank_account_iban")]
    [StringLength(24, ErrorMessage = "Bank account IBAN must be 24 characters")]
    [Required(ErrorMessage = "Bank account IBAN is required")]
    public string BankAccountIban { get; set; }

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