using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.users;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController(SessionService sessionService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserLoginDto userLogin)
    {
        return await sessionService.Login(userLogin);
    }

    [HttpPost("signup")]
    public async Task<ActionResult<string>> Register(UserCreateDto userRegister)
    {
        return await sessionService.Register(userRegister);
    }

    [HttpGet("token/{token}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<UserResponseDto>> GetUserByToken([FromRoute] string token)
    {
        return await sessionService.GetUserByToken(token);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetMyself()
    {
        return await sessionService.GetMyself();
    }
}