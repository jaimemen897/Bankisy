using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpGet("bankaccounts")]
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId()
    {
        return await indexService.GetBankAccountsByUserId();
    }
    
    [HttpGet("totalbalance")]
    public async Task<decimal> GetTotalBalanceByUserId()
    {
        return await indexService.GetTotalBalanceByUserId();
    }
    
    [HttpGet("transactions")]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetTransactionsByUserId([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false, [FromQuery] string? search = null)
    {
        return await indexService.GetTransactionsByUserId(pageNumber, pageSize, orderBy, descending, search);
    }
    
    [HttpGet("expenses")]
    public async Task<List<TransactionResponseDto>> GetExpensesByUserId()
    {
        return await indexService.GetExpensesByUserId();
    }
    
    [HttpGet("incomes")]
    public async Task<List<TransactionResponseDto>> GetIncomesByUserId()
    {
        return await indexService.GetIncomesByUserId();
    }
    
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
    
    [HttpGet("/cards/user")]
    public async Task<List<CardResponseDto>> GetCardsByUserId()
    {
        return await indexService.GetCardsByUserId();
    }
    
    [HttpGet("/cards/bankaccount/{iban}")]
    public async Task<List<CardResponseDto>> GetCardsByIban(string iban)
    {
        return await indexService.GetCardsByIban(iban);
    }
}