using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.users;
using TFG.Context.Models;

namespace TFG.Services.mappers;

public class UsersMapper
{

    public static User MapToEntity(UserCreateDto userDto)
    {
        return new User
        {
            Id = new Guid(),
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password,
            Avatar = userDto.Avatar,
            CreatedAt = DateTime.Now.ToUniversalTime(),
            UpdatedAt = DateTime.Now.ToUniversalTime(),
            BankAccounts = new List<BankAccount>()
        };
    }

    public static User MapToEntity(User user, UserUpdateDto userDto)
    {
        user.Name = userDto.Name ?? user.Name;
        user.Email = userDto.Email ?? user.Email;
        user.Avatar = userDto.Avatar ?? user.Avatar;
        user.UpdatedAt = DateTime.Now.ToUniversalTime();
        return user;
    }

    public static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}