using System.ComponentModel.DataAnnotations;
using TFG.Context.Models;

namespace TFG.Context.DTOs.bankAccount;

public class BankAccountCreateDto
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }
    [Required]
    [EnumDataType(typeof(AccountType))]
    public AccountType AccountType { get; set; }
    [Required]
    public Guid UserId { get; set; }
}