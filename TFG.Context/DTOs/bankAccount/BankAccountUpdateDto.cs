using TFG.Context.Models;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountUpdateDto
{
    public decimal? Balance { get; set; }
    public AccountType? AccountType { get; set; }
    public List<Guid>? UsersId { get; set; }
}