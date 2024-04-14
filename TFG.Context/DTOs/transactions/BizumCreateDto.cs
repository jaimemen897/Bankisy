using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.transactions;

public class BizumCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "The concept must be between 3 and 100 characters")]
    public string Concept { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public string PhoneNumberUserOrigin { get; set; }

    [Required]
    public string PhoneNumberUserDestination { get; set;}
}