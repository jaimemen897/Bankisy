using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Exceptions;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TransactionsController(TransactionService transactionService) : ControllerBase
{
    //GET
    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetTransactions([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, null, search, filter);
    }
    
    [Authorize(Policy = "User")]
    [HttpGet("myself")]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetMyTransactions([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, GetUserId(), search, filter);
    }
    
    [Authorize(Policy = "User")]
    [HttpGet("myself/incomes")]
    public async Task<List<TransactionResponseDto>> GetMyIncomes()
    {
        return await transactionService.GetIncomesByUserId(GetUserId());
    }
    
    [Authorize(Policy = "User")]
    [HttpGet("myself/expenses")]
    public async Task<List<TransactionResponseDto>> GetMyExpenses()
    {
        return await transactionService.GetExpensesByUserId(GetUserId());
    }

    [Authorize(Policy = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        return await transactionService.GetTransaction(id);
    }

    [Authorize(Policy = "Admin")]
    [HttpGet("{bankAccountIban}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(string bankAccountIban)
    {
        return await transactionService.GetTransactionsForAccount(bankAccountIban);
    }

    //CREATE
    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await transactionService.CreateTransaction(transaction);
    }

    [Authorize(Policy = "Admin")]
    [HttpPost("bizum/{userId}")]
    public async Task<ActionResult<BizumResponseDto>> CreateBizum(BizumCreateDto transaction, Guid userId)
    {
        return await transactionService.CreateBizum(transaction, userId);
    }

    //DELETE
    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task DeleteTransaction(int id)
    {
        await transactionService.DeleteTransaction(id);
    }
    
    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HttpException(401, "Unauthorized"));
    }

}