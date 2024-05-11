using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class SessionService(
    UsersService usersService,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<string> Login(UserLoginDto userLogin)
    {
        var user = await usersService.ValidateUserCredentials(userLogin.Username, userLogin.Password) ??
                   throw new HttpException(401, "Invalid credentials");

        return GetToken(user);
    }

    public async Task<string> Register(UserCreateDto userRegister)
    {
        var user = await usersService.CreateUser(userRegister) ?? throw new HttpException(400, "Error creating user");
        var userMapped = _mapper.Map<User>(user);
        return GetToken(userMapped);
    }

    public async Task<UserResponseDto> GetUserByToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        var id = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ??
                 throw new HttpException(401, "Invalid token");
        return await usersService.GetUserAsync(Guid.Parse(id)) ?? throw new HttpException(404, "User not found");
    }

    public async Task<UserResponseDto> GetMyself()
    {
        var token = httpContextAccessor.HttpContext!.Request.Headers.Authorization.FirstOrDefault()?.Split(" ")
            .Last() ?? throw new HttpException(401, "Token not found");
        return await GetUserByToken(token);
    }

    public string GetToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:secret"] ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddDays(4),
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        var response = new
        {
            token = jwtToken,
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            }
        };

        return JsonConvert.SerializeObject(response);
    }
}