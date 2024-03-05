using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.transactions;

public class TransactionCreateDto
{
    [Required]
    public string Concept { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public Guid IdAccountOrigin { get; set; }

    [Required]
    public Guid IdAccountDestination { get; set; }
}