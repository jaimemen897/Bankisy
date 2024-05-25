using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Exceptions;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController(UsersService usersService) : ControllerBase
{
    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Pagination<UserResponseDto>>> GetUsers([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Name", [FromQuery] bool descending = false,
        [FromQuery] string? search = null)
    {
        return await usersService.GetUsers(pageNumber, pageSize, orderBy, descending, search);
    }

    [Authorize(Policy = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        return await usersService.GetUserAsync(id);
    }

    [Authorize(Policy = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<UserResponseDto[]>> GetAllUsers()
    {
        return await usersService.GetAllUsers();
    }

    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto user)
    {
        return await usersService.CreateUser(user);
    }

    [Authorize(Policy = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, UserUpdateDto user)
    {
        return await usersService.UpdateUser(id, user);
    }

    [Authorize(Policy = "User")]
    [HttpPut("profile")]
    public async Task<ActionResult<string>> UpdateProfile(UserUpdateDto user)
    {
        return await usersService.UpdateProfile(GetUserId(), user);
    }

    [Authorize(Policy = "Admin")]
    [HttpPut("{id}/avatar")]
    public async Task<ActionResult<UserResponseDto>> UpdateUserAvatar(Guid id, [FromForm] IFormFile avatar)
    {
        var host = $"{Request.Scheme}://{Request.Host}";
        return await usersService.UploadAvatar(avatar, host, id);
    }
    
    [Authorize(Policy = "User")]
    [HttpPut("avatar")]
    public async Task<ActionResult<UserResponseDto>> UpdateAvatar([FromForm] IFormFile avatar)
    {
        var host = $"{Request.Scheme}://{Request.Host}";
        var userId = GetUserId();
        return await usersService.UploadAvatar(avatar, host, userId);
    }

    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}/avatar")]
    public async Task<ActionResult<UserResponseDto>> DeleteUserAvatar(Guid id)
    {
        return await usersService.DeleteAvatar(id);
    }

    [Authorize(Policy = "User")]
    [HttpDelete("avatar")]
    public async Task<ActionResult> DeleteMyAvatar()
    {
        await usersService.DeleteAvatar(GetUserId());
        return Ok();
    }

    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task DeleteUser(Guid id)
    {
        await usersService.DeleteUser(id);
    }

    private Guid GetUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HttpException(401, "Unauthorized"));
    }
}