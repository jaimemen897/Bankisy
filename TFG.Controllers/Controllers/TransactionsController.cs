using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("[controller]")]
public class TransactionsController(TransactionService transactionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetTransactions([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, null, search, filter);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        return await transactionService.GetTransaction(id);
    }

    [HttpGet("{bankAccountIban}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(string bankAccountIban)
    {
        return await transactionService.GetTransactionsForAccount(bankAccountIban);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await transactionService.CreateTransaction(transaction);
    }

    [HttpPost("bizum/{userId}")]
    public async Task<ActionResult<BizumResponseDto>> CreateBizum(BizumCreateDto transaction, Guid userId)
    {
        return await transactionService.CreateBizum(transaction, userId);
    }

    [HttpDelete("{id}")]
    public async Task DeleteTransaction(int id)
    {
        await transactionService.DeleteTransaction(id);
    }
}