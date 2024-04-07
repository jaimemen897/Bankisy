using System.ComponentModel.DataAnnotations;
using TFG.Context.Models;

namespace TFG.Context.DTOs.users;

public class UserCreateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }
    [Required]
    [StringLength(9, MinimumLength = 9)]
    public string Dni { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Password { get; set; }
    public string? Avatar { get; set; }
    [Required]
    [StringLength(9, MinimumLength = 9)]
    public string Phone { get; set; }
}