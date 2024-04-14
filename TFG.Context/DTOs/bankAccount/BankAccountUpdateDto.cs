using TFG.Context.Models;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountUpdateDto
{
    public string? AccountType { get; set; }
    public List<Guid>? UsersId { get; set; }
    public bool? AcceptBizum { get; set; }
}