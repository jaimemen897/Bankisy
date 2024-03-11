using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountCreateDto
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }
    [Required]
    public string AccountType { get; set; }
    [Required]
    public List<Guid> UsersId { get; set; }
}