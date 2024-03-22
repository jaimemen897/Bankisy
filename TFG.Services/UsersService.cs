using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class UsersService(BankContext bankContext, IMemoryCache cache)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<UserResponseDto>> GetUsers(int pageNumber, int pageSize, string orderBy,
        bool descending, string? search = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(UserResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        /*var cacheKey = $"GetUsers-{pageNumber}-{pageSize}-{orderBy}-{descending}";
        if (cache.TryGetValue(cacheKey, out Pagination<UserResponseDto>? users))
        {
            if (users != null) return users;
        }*/

        var usersQuery = bankContext.Users.Where(user => !user.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            usersQuery = usersQuery.Where(user =>
                user.Name.ToLower().Contains(search.ToLower()) || user.Email.ToLower().Contains(search.ToLower()));
        }

        var paginatedUsers = await usersQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            user => _mapper.Map<UserResponseDto>(user));

        /*var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, paginatedUsers, cacheEntryOptions);*/

        return paginatedUsers;
    }

    public async Task<UserResponseDto> GetUserAsync(Guid id)
    {
        var cacheKey = $"GetUser-{id}";
        if (cache.TryGetValue(cacheKey, out UserResponseDto? user))
        {
            if (user != null) return user;
        }

        var userEntity = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        user = _mapper.Map<UserResponseDto>(userEntity);
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, user, cacheEntryOptions);

        return user ?? throw new HttpException(404, "User not found");
    }

    private static void IsValid(UserCreateDto userCreateDto)
    {
        if (!Enum.TryParse(typeof(Gender), userCreateDto.Gender, true, out _))
        {
            throw new HttpException(400,
                "Invalid gender. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(Gender))));
        }
    }

    private static void IsValid(UserUpdateDto userUpdateDto)
    {
        if (!Enum.TryParse(typeof(Gender), userUpdateDto.Gender, true, out _))
        {
            throw new HttpException(400,
                "Invalid gender. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(Gender))));
        }
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        IsValid(user);
        var userDto = _mapper.Map<User>(user);
        await bankContext.Users.AddAsync(userDto);
        await bankContext.SaveChangesAsync();

        await ClearCache();

        return _mapper.Map<UserResponseDto>(userDto);
    }

    public async Task<UserResponseDto> UpdateUser(Guid id, UserUpdateDto user)
    {
        IsValid(user);
        var userToUpdate = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");

        userToUpdate = _mapper.Map(user, userToUpdate);
        await bankContext.SaveChangesAsync();

        await ClearCache();

        return _mapper.Map<UserResponseDto>(userToUpdate);
    }

    public async Task DeleteUser(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        if (user.Avatar != User.ImageDefault)
        {
            var avatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads",
                user.Avatar.Split("/").Last());
            if (File.Exists(avatar))
            {
                File.Delete(avatar);
            }
        }


        var bankAccounts = await bankContext.BankAccounts.Where(ba => ba.UsersId.Contains(user)).ToListAsync();
        bankAccounts.ForEach(ba => ba.IsDeleted = true);
        user.IsDeleted = true;
        await bankContext.SaveChangesAsync();

        await ClearCache();
    }

    public async Task<UserResponseDto> UploadAvatar(Guid id, IFormFile file, string host)
    {
        var user = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploads))
        {
            Directory.CreateDirectory(uploads);
        }

        if (file.Length > 0)
        {
            /*check if file is an image*/
            if (!file.ContentType.Contains("image"))
            {
                throw new HttpException(400, "Invalid file type. Only images are allowed");
            }
            
            if (user.Avatar != User.ImageDefault)
            {
                var avatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads",
                    user.Avatar.Split("/").Last());
                if (File.Exists(avatar))
                {
                    File.Delete(avatar);
                }
            }
            
            var filePath = Path.Combine(uploads, user.Id + "-" + file.FileName);
            
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            user.Avatar = $"{host}/uploads/{user.Id}-{file.FileName}";
            bankContext.Entry(user).State = EntityState.Modified;
            await bankContext.SaveChangesAsync();
        }

        await ClearCache();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<User> ValidateUserCredentials(string username, string password)
    {
        return await bankContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password) ??
               throw new HttpException(400, "Invalid credentials");
    }

    private async Task ClearCache()
    {
        var ids = await bankContext.Users.Select(u => u.Id).ToListAsync();
        foreach (var id in ids)
        {
            cache.Remove("GetUser-" + id);
        }
    }
}