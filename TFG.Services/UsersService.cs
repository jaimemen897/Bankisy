using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Services.mappers;

namespace TFG.Services;

public class UsersService
{
    private readonly BankContext _bankContext;

    public UsersService(BankContext bankContext)
    {
        _bankContext = bankContext;
    }

    public async Task<List<UserResponseDto>> GetUsers()
    {
        var userList = await _bankContext.Users.ToListAsync();
        return userList.Select(UsersMapper.MapToResponseDto).ToList();
    }
    
    public async Task<UserResponseDto?> GetUserAsync(Guid id)
    {
        var user = await _bankContext.Users.FindAsync(id);
        return user == null ? null: UsersMapper.MapToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        var userDto = UsersMapper.MapToEntity(user);
        await _bankContext.Users.AddAsync(userDto);
        await _bankContext.SaveChangesAsync();
        return UsersMapper.MapToResponseDto(userDto);
    }
    
    public async Task<UserResponseDto?> UpdateUser(Guid id, UserUpdateDto user)
    {
        var userToUpdate = await _bankContext.Users.FindAsync(id);
        if (userToUpdate == null)
        {
            return null;
        }
        
        userToUpdate = UsersMapper.MapToEntity(userToUpdate, user);
        await _bankContext.SaveChangesAsync();
        
        return UsersMapper.MapToResponseDto(userToUpdate);
    }
    
    public async Task<bool> DeleteUser(Guid id)
    {
        var user = await _bankContext.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _bankContext.Users.Remove(user);
        await _bankContext.SaveChangesAsync();

        return true;
    }
}