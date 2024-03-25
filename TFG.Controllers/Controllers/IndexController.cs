using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class IndexController(IndexService indexService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUser(Guid userId)
    {
        return await indexService.GetBankAccountsByUserId(userId);
    }
    
    [HttpGet("user/{userId}/totalbalance")]
    public async Task<decimal> GetTotalBalanceByUserId(Guid userId)
    {
        return await indexService.GetTotalBalanceByUserId(userId);
    }
    
    [HttpGet("user/{userId}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsByUserId(Guid userId)
    {
        return await indexService.GetTransactionsByUserId(userId);
    }
    
    [HttpGet("user/{userId}/expenses")]
    public async Task<List<TransactionResponseDto>> GetExpensesByUserId(Guid userId)
    {
        return await indexService.GetExpensesByUserId(userId);
    }
    
    [HttpGet("user/{userId}/incomes")]
    public async Task<List<TransactionResponseDto>> GetIncomesByUserId(Guid userId)
    {
        return await indexService.GetIncomesByUserId(userId);
    }
}