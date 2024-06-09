using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("users")]
public class User
{
    public const string ImageDefault = "assets/avatar.png";

    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("name")]
    [DataType(DataType.Text)]
    [MaxLength(100, ErrorMessage = "Name must be less than 100 characters")]
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Column("email")]
    [DataType(DataType.EmailAddress)]
    [MaxLength(100, ErrorMessage = "Email must be less than 100 characters")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }

    [Column("username")]
    [DataType(DataType.Text)]
    [MaxLength(100, ErrorMessage = "Username must be less than 100 characters")]
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }

    [Column("dni")]
    [DataType(DataType.Text)]
    [StringLength(9, ErrorMessage = "DNI must be 9 characters")]
    [Required(ErrorMessage = "DNI is required")]
    public string Dni { get; set; }

    //integer
    [Column("gender")] [Range(0, 3)] public Gender Gender { get; set; }

    [Column("password")]
    [DataType(DataType.Password)]
    [MaxLength(100, ErrorMessage = "Password must be less than 100 characters")]
    public string Password { get; set; }

    [Column("avatar")]
    [DataType(DataType.ImageUrl)]
    [MaxLength(100, ErrorMessage = "Avatar must be less than 100 characters")]
    public string Avatar { get; set; } = ImageDefault;

    [Column("phone")]
    [DataType(DataType.PhoneNumber)]
    [StringLength(9, ErrorMessage = "Phone must be 9 characters")]
    public string Phone { get; set; }

    [Column("role")] [Range(0, 1)] public Roles Role { get; set; }

    [Column("is_deleted")] public bool IsDeleted { get; set; }

    [Column("created_at")]
    [DataType(DataType.DateTime)]
    public DateTime? CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    [Column("updated_at")]
    [DataType(DataType.DateTime)]
    public DateTime? UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    public List<BankAccount> BankAccounts { get; set; } = [];

    public List<Card> Cards { get; set; } = [];
}

public enum Roles
{
    Admin,
    User
}

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}