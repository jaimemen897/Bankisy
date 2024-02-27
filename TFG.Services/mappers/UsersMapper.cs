using TFG.Context.DTOs;
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
        };
    }
    
    public static User MapToEntity(UserUpdateDto userDto)
    {
        return new User
        {
            Id = userDto.Id,
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password,
            Avatar = userDto.Avatar,
        };
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