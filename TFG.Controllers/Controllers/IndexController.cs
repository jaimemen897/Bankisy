using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpPost("transaction")]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await indexService.CreateTransaction(transaction);
    }

    [HttpGet("{iban}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsByIban(string iban)
    {
        return await indexService.GetTransactionsByIban(iban);
    }

    [HttpGet("cards/bankaccount/{iban}")]
    public async Task<List<CardResponseDto>> GetCardsByIban(string iban)
    {
        return await indexService.GetCardsByIban(iban);
    }

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

    [HttpPost("transaction/bizum")]
    public async Task<ActionResult<BizumResponseDto>> CreateBizum(BizumCreateDto transaction)
    {
        return await indexService.CreateBizum(transaction);
    }
}