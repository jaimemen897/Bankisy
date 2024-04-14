using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountCreateDto
{
    [Required]
    public string AccountType { get; set; }
    [Required]
    public List<Guid> UsersId { get; set; }
    [Required]
    public bool AcceptBizum { get; set; }
}