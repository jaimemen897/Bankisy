using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class UsersService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<List<UserResponseDto>> GetUsers()
    {
        var userList = await bankContext.Users.Include(u => u.BankAccounts).ToListAsync();
        return userList.Where(user => !user.IsDeleted).Select(user => _mapper.Map<UserResponseDto>(user)).ToList();
    }

    public async Task<UserResponseDto> GetUserAsync(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        var userDto = _mapper.Map<User>(user);
        await bankContext.Users.AddAsync(userDto);
        await bankContext.SaveChangesAsync();
        return _mapper.Map<UserResponseDto>(userDto);
    }

    public async Task<UserResponseDto> UpdateUser(Guid id, UserUpdateDto user)
    {
        var userToUpdate = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");

        userToUpdate = _mapper.Map(user, userToUpdate);
        await bankContext.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(userToUpdate);
    }

    public async Task DeleteUser(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        var bankAccounts = await bankContext.BankAccounts.Where(ba => ba.UserId == id).ToListAsync();
        bankAccounts.ForEach(ba => ba.IsDeleted = true);
        user.IsDeleted = true;
        await bankContext.SaveChangesAsync();
    }
}