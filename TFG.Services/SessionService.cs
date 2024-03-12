using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TFG.Context.DTOs.users;
using TFG.Services.Exceptions;

namespace TFG.Services;

public class SessionService(UsersService usersService)
{
    public async Task<string> Login(UserLoginDto userLogin)
    {
        var user = await usersService.ValidateUserCredentials(userLogin.Email, userLogin.Password);
        if (user is null)
        {
            throw new HttpException(401, "Invalid credentials");
        }

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