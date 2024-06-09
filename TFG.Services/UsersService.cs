using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.Mappers;
using TFG.Services.Pagination;
using Microsoft.Extensions.Configuration;

namespace TFG.Services;

public class UsersService(
    BankContext bankContext,
    IMemoryCache cache,
    BankAccountService bankAccountService,
    IConfiguration configuration)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    private readonly List<Guid> _userIds = [];

    public async Task<Pagination<UserResponseDto>> GetUsers(int pageNumber, int pageSize, string orderBy,
        bool descending, string? search = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(UserResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
            throw new HttpException(400, "Invalid orderBy parameter");

        var usersQuery = bankContext.Users.Where(user => !user.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            usersQuery = usersQuery.Where(user =>
                user.Name.ToLower().Contains(search.ToLower()) || user.Email.ToLower().Contains(search.ToLower()) ||
                user.Phone.Contains(search));

        var paginatedUsers = await usersQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            user => _mapper.Map<UserResponseDto>(user));

        return paginatedUsers;
    }

    public async Task<UserResponseDto[]> GetAllUsers()
    {
        var users = await bankContext.Users.Where(u => !u.IsDeleted).ToListAsync();
        return _mapper.Map<UserResponseDto[]>(users);
    }

    public async Task<UserResponseDto> GetUserAsync(Guid id)
    {
        var cacheKey = $"GetUser-{id}";
        if (cache.TryGetValue(cacheKey, out UserResponseDto? user) && user != null) return user;

        var userEntity = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == id) ??
                         throw new HttpException(404, "User not found");
        user = _mapper.Map<UserResponseDto>(userEntity);
        AddUserToCache(userEntity);

        return user ?? throw new HttpException(404, "User not found");
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto user)
    {
        var userDto = _mapper.Map<User>(user);
        await IsValid(user);
        await bankContext.Users.AddAsync(userDto);
        await bankContext.SaveChangesAsync();
        ClearCache();
        return _mapper.Map<UserResponseDto>(userDto);
    }

    public async Task<UserResponseDto> UpdateUser(Guid id, UserUpdateDto user)
    {
        var userToUpdate = await bankContext.Users.FirstOrDefaultAsync(x => x.Id == id) ??
                           throw new HttpException(404, "User not found");
        userToUpdate = _mapper.Map(user, userToUpdate);
        await IsValid(user, userToUpdate);

        await bankContext.SaveChangesAsync();

        if (!_userIds.Contains(id)) _userIds.Add(id);

        ClearCache();

        return _mapper.Map<UserResponseDto>(userToUpdate);
    }

    public async Task<string> UpdateProfile(Guid id, UserUpdateDto user)
    {
        var userUpdated = await UpdateUser(id, user);
        var userMapped = _mapper.Map<User>(userUpdated);
        return SessionService.GetToken(userMapped, configuration);
    }

    public async Task DeleteUser(Guid id)
    {
        var user = await bankContext.Users.FindAsync(id) ?? throw new HttpException(404, "User not found");
        if (user.Avatar != User.ImageDefault)
        {
            var avatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads",
                user.Avatar.Split("/").Last());
            if (File.Exists(avatar)) File.Delete(avatar);
        }

        var bankAccounts = await bankContext.BankAccounts.Where(ba => ba.Users.Contains(user)).ToListAsync();
        if (bankAccounts.Count >= 0)
        {
            foreach (var bankAccount in bankAccounts) await bankAccountService.DeleteBankAccount(bankAccount.Iban);
            user.IsDeleted = true;
        }
        else
        {
            bankContext.Users.Remove(user);
        }

        await bankContext.SaveChangesAsync();

        ClearCache();
    }

    public async Task<UserResponseDto> UploadAvatar(IFormFile file, string host, Guid id)
    {
        var user = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == id) ??
                   throw new HttpException(404, "User not found");
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

        if (file.Length > 0)
        {
            if (!file.ContentType.Contains("image"))
                throw new HttpException(400, "Invalid file type. Only images are allowed");

            if (user.Avatar != User.ImageDefault)
            {
                var avatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads",
                    user.Avatar.Split("/").Last());
                if (File.Exists(avatar)) File.Delete(avatar);
            }

            var filePath = Path.Combine(uploads, user.Id + "-" + file.FileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            user.Avatar = $"{host}/uploads/{user.Id}-{file.FileName}";
            bankContext.Users.Update(user);
            await bankContext.SaveChangesAsync();
        }

        ClearCache();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> DeleteAvatar(Guid id)
    {
        var user = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == id) ??
                   throw new HttpException(404, "User not found");
        if (user.Avatar != User.ImageDefault)
        {
            var avatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads",
                user.Avatar.Split("/").Last());
            if (File.Exists(avatar)) File.Delete(avatar);

            user.Avatar = User.ImageDefault;

            bankContext.Users.Update(user);
            await bankContext.SaveChangesAsync();
        }

        ClearCache();

        return _mapper.Map<UserResponseDto>(user);
    }

    private async Task IsValid(UserCreateDto userCreateDto)
    {
        if (!Enum.TryParse(typeof(Gender), userCreateDto.Gender, true, out _))
            throw new HttpException(400,
                "Invalid gender. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(Gender))));

        var userExists = await bankContext.Users.AnyAsync(u =>
            u.Username == userCreateDto.Username ||
            u.Email == userCreateDto.Email ||
            u.Dni == userCreateDto.Dni);

        if (userExists)
            throw new HttpException(400, "Username, Email or DNI already exists");
    }

    private async Task IsValid(UserUpdateDto userUpdateDto, User user)
    {
        if (userUpdateDto.Gender != null && !Enum.TryParse(typeof(Gender), userUpdateDto.Gender, true, out _))
            throw new HttpException(400,
                "Invalid gender. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(Gender))));

        var userExists = await bankContext.Users.AnyAsync(u =>
            (userUpdateDto.Username != null && user.Username != userUpdateDto.Username &&
             u.Username == userUpdateDto.Username) ||
            (userUpdateDto.Dni != null && user.Dni != userUpdateDto.Dni && u.Dni == userUpdateDto.Dni) ||
            (userUpdateDto.Email != null && user.Email != userUpdateDto.Email && u.Email == userUpdateDto.Email));

        if (userExists)
            throw new HttpException(400, "Username, DNI or Email already exists");
    }

    public async Task<User> ValidateUserCredentials(string username, string password)
    {
        var user = await bankContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) throw new HttpException(400, "User not found");

        if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$") &&
            !user.Password.StartsWith("$2x$") && !user.Password.StartsWith("$2y$"))
            throw new HttpException(400, "Invalid password hash");

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) throw new HttpException(400, "Invalid credentials");

        return user;
    }

    private void AddUserToCache(User user)
    {
        var cacheKey = $"GetUser-{user.Id}";
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        _userIds.Add(user.Id);
        cache.Set(cacheKey, user, cacheEntryOptions);
    }

    private void ClearCache()
    {
        foreach (var cacheKey in _userIds.Select(id => $"GetUser-{id}")) cache.Remove(cacheKey);
    }
}