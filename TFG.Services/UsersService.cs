using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.mappers;

namespace TFG.Services;

public class UsersService
{
    private readonly BankContext _bankContext;
    private readonly Mapper _mapper;

    public UsersService(BankContext bankContext)
    {
        _bankContext = bankContext;
        _mapper = MapperConfig.InitializeAutomapper();
    }

    public async Task<List<UserResponseDto>> GetUsers()
    {
        var userList = await _bankContext.Users.Include(u => u.BankAccounts).ToListAsync();
        return userList.Select(user => _mapper.Map<UserResponseDto>(user)).ToList();
    }

    public async Task<ActionResult<UserResponseDto>> GetUserAsync(Guid id)
    {
        var user = await _bankContext.Users.FindAsync(id);
        return user == null ? new NotFoundResult() : _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        var userDto = _mapper.Map<User>(user);
        await _bankContext.Users.AddAsync(userDto);
        await _bankContext.SaveChangesAsync();
        return _mapper.Map<UserResponseDto>(userDto);
    }

    public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, UserUpdateDto user)
    {
        var userToUpdate = await _bankContext.Users.FindAsync(id);
        if (userToUpdate == null)
        {
            return new NotFoundResult();
        }

        userToUpdate = _mapper.Map(user, userToUpdate);
        await _bankContext.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(userToUpdate);
    }

    public async Task<ActionResult<bool>> DeleteUser(Guid id)
    {
        var user = await _bankContext.Users.FindAsync(id);
        if (user == null)
        {
            return new NotFoundResult();
        }

        _bankContext.Users.Remove(user);
        await _bankContext.SaveChangesAsync();

        return true;
    }
}