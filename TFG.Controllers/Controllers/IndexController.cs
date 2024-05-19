using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpPost("bankaccount")]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await indexService.CreateBankAccount(bankAccount);
    }

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

    [HttpGet("card/{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> GetCardByCardNumber(string cardNumber)
    {
        return await indexService.GetCardByCardNumber(cardNumber);
    }

    [HttpPost("card")]
    public async Task<ActionResult<CardResponseDto>> CreateCard(CardCreateDto cardCreateDto)
    {
        return await indexService.CreateCard(cardCreateDto);
    }

    [HttpPut("card/{cardNumber}")]
    public async Task<ActionResult<CardResponseDto>> UpdateCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        return await indexService.UpdateCard(cardNumber, cardUpdateDto);
    }

    [HttpDelete("card/{cardNumber}")]
    public async Task<ActionResult> DeleteCard(string cardNumber)
    {
        await indexService.DeleteCard(cardNumber);
        return Ok();
    }

    [HttpPost("card/{cardNumber}/renovate")]
    public async Task<ActionResult<CardResponseDto>> RenovateCard(string cardNumber)
    {
        return await indexService.RenovateCard(cardNumber);
    }

    [HttpPost("card/{cardNumber}/block")]
    public async Task<ActionResult> BlockCard(string cardNumber)
    {
        await indexService.BlockCard(cardNumber);
        return Ok();
    }

    [HttpPost("card/{cardNumber}/unblock")]
    public async Task<ActionResult> UnblockCard(string cardNumber)
    {
        await indexService.UnblockCard(cardNumber);
        return Ok();
    }

    [HttpPost("card/{cardNumber}/activate")]
    public async Task<ActionResult> ActivateCard(string cardNumber)
    {
        await indexService.ActivateCard(cardNumber);
        return Ok();
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

    [HttpPost("bankaccount/{iban}/active-bizum")]
    public async Task<ActionResult> ActiveBizum(string iban)
    {
        await indexService.ActiveBizum(iban);
        return Ok();
    }

    [HttpPost("transaction/bizum")]
    public async Task<ActionResult<BizumResponseDto>> CreateBizum(BizumCreateDto transaction)
    {
        return await indexService.CreateBizum(transaction);
    }
}