using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.transactions;

public class IncomeCreateDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required] public string IbanAccountDestination { get; set; }
}