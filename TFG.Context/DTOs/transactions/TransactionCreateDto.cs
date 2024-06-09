using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.transactions;

public class TransactionCreateDto
{
    [Required]
    [MaxLength(255, ErrorMessage = "The concept must be less than 255 characters")]
    [MinLength(1, ErrorMessage = "The concept must be at least 1 character")]
    public string Concept { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(24, ErrorMessage = "The Iban account origin must be 24 characters long", MinimumLength = 24)]
    public string IbanAccountOrigin { get; set; }

    [Required]
    [StringLength(24, ErrorMessage = "The Iban account destination must be 24 characters long", MinimumLength = 24)]
    public string IbanAccountDestination { get; set; }
}