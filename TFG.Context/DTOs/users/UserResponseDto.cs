using TFG.Context.DTOs.bankAccount;

namespace TFG.Context.DTOs.users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<BankAccountResponseDto> BankAccounts { get; set; }
}