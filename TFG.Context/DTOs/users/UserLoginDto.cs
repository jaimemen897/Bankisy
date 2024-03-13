using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.users;

public class UserLoginDto
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Password { get; set; }
}