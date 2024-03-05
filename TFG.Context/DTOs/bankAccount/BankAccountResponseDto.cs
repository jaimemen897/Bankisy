using TFG.Context.DTOs.transactions;
using TFG.Context.Models;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public AccountType AccountType { get; set; }
    public Guid UserId { get; set; }
}