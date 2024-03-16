using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class SessionService(UsersService usersService)
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
    
    private static string GetToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new HttpException(500, "JWT_SECRET not found")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
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