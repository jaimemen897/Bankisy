namespace TFG.Context.DTOs.users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}