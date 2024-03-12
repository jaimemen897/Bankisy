using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpGet()]
    public async Task<ActionResult<Pagination<UserResponseDto>>> GetUsers([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false)
    {
        return await usersService.GetUsers(pageNumber, pageSize, orderBy, descending);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        return await usersService.GetUserAsync(id);
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

    [HttpDelete("{id}")]
    public async Task DeleteUser(Guid id)
    {
        await usersService.DeleteUser(id);
    }
}