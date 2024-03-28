using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.users;

namespace TFG.Context.DTOs.cards;

public class CardResponseDto
{
    public string CardNumber { get; set; }
    public string Pin { get; set; }
    public string CardType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Cvv { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsBlocked { get; set; }
    public UserResponseDto User { get; set; }
    public BankAccountResponseDto BankAccount { get; set; }
}