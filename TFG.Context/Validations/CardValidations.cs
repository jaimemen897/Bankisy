using System.ComponentModel.DataAnnotations;

namespace TFG.Context.Validations;

public class CardValidations
{
    [StringLength(4)]
    public virtual string? Pin { get; set; }

    public virtual string? CardType { get; set; }

    public virtual Guid? UserId { get; set; }

    [StringLength(24, ErrorMessage = "Bank account IBAN must be 24 characters")]
    public virtual string? BankAccountIban { get; set; }
}