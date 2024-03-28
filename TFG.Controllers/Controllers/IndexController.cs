using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpGet("{userId}/bankaccounts")]
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUser(Guid userId)
    {
        return await indexService.GetBankAccountsByUserId(userId);
    }
    
    [HttpGet("{userId}/totalbalance")]
    public async Task<decimal> GetTotalBalanceByUserId(Guid userId)
    {
        return await indexService.GetTotalBalanceByUserId(userId);
    }
    
    [HttpGet("transactions/{userId}")]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetTransactionsByUserId(Guid userId, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false, [FromQuery] string? search = null)
    {
        return await indexService.GetTransactionsByUserId(userId, pageNumber, pageSize, orderBy, descending, search);
    }
    
    
    [HttpGet("{userId}/expenses")]
    public async Task<List<TransactionResponseDto>> GetExpensesByUserId(Guid userId)
    {
        return await indexService.GetExpensesByUserId(userId);
    }
    
    [HttpGet("{userId}/incomes")]
    public async Task<List<TransactionResponseDto>> GetIncomesByUserId(Guid userId)
    {
        return await indexService.GetIncomesByUserId(userId);
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
}