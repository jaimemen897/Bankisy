using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("users")]
public class User
{
    public const string ImageDefault = "assets/avatar.png";

    [Column("id"), Key] public Guid Id { get; set; }

    [Column("name")] public string Name { get; set; }

    [Column("email")] public string Email { get; set; }
    
    [Column("username")] public string Username { get; set; }
    
    [Column("dni")] public string Dni { get; set; }
    
    [Column("gender")] public Gender Gender { get; set; }

    [Column("password")] public string Password { get; set; }

    [Column("avatar")] public string Avatar { get; set; } = ImageDefault;
    
    [Column("role")] public Roles Role { get; set; }

    [Column("is_deleted")] public bool IsDeleted { get; set; }

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();

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