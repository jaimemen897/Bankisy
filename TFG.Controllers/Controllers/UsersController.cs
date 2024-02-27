using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs;
using TFG.Context.DTOs.users;
using TFG.Services;

namespace TFG.Controllers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet()]
        public async Task<ActionResult<List<UserResponseDto>>> GetUsers()
        {
            return await _usersService.GetUsers();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            var user = await _usersService.GetUserAsync(id);
            return user == null ? NotFound() : user;
        }

        [HttpPost()]
        public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto user)
        {
            return await _usersService.CreateUser(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, UserUpdateDto user)
        {
            var userUpdated = await _usersService.UpdateUser(id, user);
            return userUpdated == null ? NotFound() : userUpdated;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            return await _usersService.DeleteUser(id) ? Ok() : NotFound();
        }
    }
}