using Microsoft.AspNetCore.Mvc;
using TFG.Context.Models;
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
        public List<User> GetUsers()
        {
            return _usersService.GetUsers();
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(Guid id)
        {
            var user = _usersService.GetUser(id);
            return user == null ? NotFound() : user;
        }
        
        [HttpPost()]
        public ActionResult<User> CreateUser(User user)
        {
            var createdUser = _usersService.CreateUser(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        
        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser(Guid id, User user)
        {
            var updatedUser = _usersService.UpdateUser(id, user);
            return updatedUser == null ? NotFound() : updatedUser;
        }
        
        [HttpDelete("{id}")]
        public ActionResult DeleteUser(Guid id)
        {
            return _usersService.DeleteUser(id) ? NoContent() : NotFound();
        }
    }
}