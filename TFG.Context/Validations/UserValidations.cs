using System.ComponentModel.DataAnnotations;

namespace TFG.Context.Validations;

public class UserValidations
{
    [DataType(DataType.Text)]
    [MaxLength(100, ErrorMessage = "Name must be less than 100 characters")]
    [MinLength(3, ErrorMessage = "Name must be more than 3 characters")]
    public virtual string? Name { get; set; }

    [DataType(DataType.EmailAddress)]
    [MaxLength(100, ErrorMessage = "Email must be less than 100 characters")]
    [MinLength(3, ErrorMessage = "Email must be more than 3 characters")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public virtual string? Email { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(100, ErrorMessage = "Username must be less than 100 characters")]
    [MinLength(3, ErrorMessage = "Username must be more than 3 characters")]
    public virtual string? Username { get; set; }

    [DataType(DataType.Text)]
    [StringLength(9, ErrorMessage = "DNI must be 9 characters")]
    public virtual string? Dni { get; set; }

    public virtual string? Gender { get; set; }

    [DataType(DataType.Password)]
    [MaxLength(100, ErrorMessage = "Password must be less than 100 characters")]
    public virtual string? Password { get; set; }

    [DataType(DataType.ImageUrl)]
    [MaxLength(100, ErrorMessage = "Avatar must be less than 100 characters")]
    public virtual string? Avatar { get; set; }

    [DataType(DataType.PhoneNumber)]
    [StringLength(9, ErrorMessage = "Phone must be 9 characters")]
    public virtual string? Phone { get; set; }
}