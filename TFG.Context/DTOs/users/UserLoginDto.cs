using System.ComponentModel.DataAnnotations;

namespace TFG.Context.DTOs.users;

public class UserLoginDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Username { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Password { get; set; }
}