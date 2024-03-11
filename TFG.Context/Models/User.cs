using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFG.Context.Models;

[Table("users")]
public class User
{
    [Column("id"), Key]
    public Guid Id { get; set; } = new Guid();

    [Column("name")]
    public string Name { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("password")]
    public string Password { get; set; }

    [Column("avatar")]
    public string Avatar { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    
    public ICollection<BankAccount> BankAccounts { get; set; }
 
    
    public User()
    {
        BankAccounts = new List<BankAccount>();
    }
}