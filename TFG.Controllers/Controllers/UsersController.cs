using Microsoft.AspNetCore.Mvc;
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
            return await _usersService.GetUserAsync(id);
        }

        [HttpPost()]
        public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto user)
        {
            return await _usersService.CreateUser(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, UserUpdateDto user)
        {
            return await _usersService.UpdateUser(id, user);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteUser(Guid id)
        {
            return await _usersService.DeleteUser(id);
        }
    }
}