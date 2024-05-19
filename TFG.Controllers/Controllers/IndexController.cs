using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.users;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpPut("profile")]
    public async Task<string> UpdateProfile(UserUpdateDto userUpdateDto)
    {
        return await indexService.UpdateProfile(userUpdateDto);
    }

    [HttpPut("avatar")]
    public async Task<ActionResult<UserResponseDto>> UpdateAvatar([FromForm] IFormFile avatar)
    {
        var host = $"{Request.Scheme}://{Request.Host}";
        return await indexService.UploadAvatar(avatar, host);
    }

    [HttpDelete("avatar")]
    public async Task<ActionResult> DeleteAvatar()
    {
        await indexService.DeleteAvatar();
        return Ok();
    }
}