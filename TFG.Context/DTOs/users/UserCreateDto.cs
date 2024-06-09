using System.ComponentModel.DataAnnotations;
using TFG.Context.Validations;

namespace TFG.Context.DTOs.users;

public class UserCreateDto : UserValidations
{
    [Required] public override string Name { get; set; }

    [Required] public override string Email { get; set; }

    [Required] public override string Username { get; set; }

    [Required] public override string Dni { get; set; }

    [Required] public override string Gender { get; set; }

    [Required] public override string Password { get; set; }

    [Required] public override string Phone { get; set; }
}