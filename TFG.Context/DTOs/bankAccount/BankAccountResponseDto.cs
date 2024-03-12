using TFG.Context.Models;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public AccountType AccountType { get; set; }
    public Roles Role { get; set; }
    public List<Guid> UsersId { get; set; }
    public bool IsDeleted { get; set; }
}