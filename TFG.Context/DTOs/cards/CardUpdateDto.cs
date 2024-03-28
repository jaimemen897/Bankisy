namespace TFG.Context.DTOs.cards;

public class CardUpdateDto
{
    public string? Pin { get; set; }
    public string? CardType { get; set; }
    public Guid? UserId { get; set; }
    public string? BankAccountIban { get; set; }
}