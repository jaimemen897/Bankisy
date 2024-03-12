using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.transactions;

public class TransactionCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "The concept must be between 3 and 100 characters")]
    public string Concept { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public Guid IdAccountOrigin { get; set; }

    [Required]
    public Guid IdAccountDestination { get; set; }
}