using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = "Admin")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpGet()]
    public async Task<ActionResult<Pagination<UserResponseDto>>> GetUsers([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Name", [FromQuery] bool descending = false, [FromQuery] string? search = null)
    {
        return await usersService.GetUsers(pageNumber, pageSize, orderBy, descending, search);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        return await usersService.GetUserAsync(id);
    }
    
    [HttpGet("all")]
    public async Task<ActionResult<UserResponseDto[]>> GetAllUsers()
    {
        return await usersService.GetAllUsers();
    }

    [HttpPost()]
    public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto user)
    {
        return await usersService.CreateUser(user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, UserUpdateDto user)
    {
        return await usersService.UpdateUser(id, user);
    }
    
    [HttpPut("{id}/avatar")]
    public async Task<ActionResult<UserResponseDto>> UpdateUserAvatar(Guid id, [FromForm] IFormFile avatar)
    {
        var host = $"{Request.Scheme}://{Request.Host}";
        return await usersService.UploadAvatar(id, avatar, host);
    }

    [HttpDelete("{id}")]
    public async Task DeleteUser(Guid id)
    {
        await usersService.DeleteUser(id);
    }
}