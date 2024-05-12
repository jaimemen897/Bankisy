using System.ComponentModel.DataAnnotations;
using TFG.Context.Validations;

namespace TFG.Context.DTOs.cards;

public class CardCreateDto : CardValidations
{
    [Required]
    public override string Pin { get; set; }

    [Required]
    public override string CardType { get; set; }

    [Required]
    public override Guid? UserId { get; set; }

    [Required]
    public override string BankAccountIban { get; set; }
}