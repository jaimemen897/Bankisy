using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.cards;

public class CardCreateDto
{
    [Required, StringLength(4)] public string Pin { get; set; }
    
    [Required] public string CardType { get; set; }
    
    [Required] public Guid UserId { get; set; }
    
    [Required] public string BankAccountIban { get; set; }
}