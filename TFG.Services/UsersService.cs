using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Services.mappers;

namespace TFG.Services;

public class UsersService(BankContext bankContext)
{
    public async Task<List<UserResponseDto>> GetUsers()
    {
        var userList = await bankContext.Users.Include(u => u.BankAccounts).ToListAsync();
        return userList.Select(UsersMapper.MapToResponseDto).ToList();
    }
    
    public async Task<UserResponseDto?> GetUserAsync(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id);
        return user == null ? null: UsersMapper.MapToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        var userDto = UsersMapper.MapToEntity(user);
        await bankContext.Users.AddAsync(userDto);
        await bankContext.SaveChangesAsync();
        return UsersMapper.MapToResponseDto(userDto);
    }
    
    public async Task<UserResponseDto?> UpdateUser(Guid id, UserUpdateDto user)
    {
        var userToUpdate = await bankContext.Users.FindAsync(id);
        if (userToUpdate == null)
        {
            return null;
        }
        
        userToUpdate = UsersMapper.MapToEntity(userToUpdate, user);
        await bankContext.SaveChangesAsync();
        
        return UsersMapper.MapToResponseDto(userToUpdate);
    }
    
    public async Task<bool> DeleteUser(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        bankContext.Users.Remove(user);
        await bankContext.SaveChangesAsync();

        return true;
    }
}