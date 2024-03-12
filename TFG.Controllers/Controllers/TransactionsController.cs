using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsController(TransactionService transactionService) : ControllerBase
{
    [HttpGet()]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetTransactions([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        return await transactionService.GetTransaction(id);
    }

    [HttpPost()]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await transactionService.CreateTransaction(transaction);
    }

    [HttpDelete("{id}")]
    public async Task DeleteTransaction(int id)
    {
        await transactionService.DeleteTransaction(id);
    }
}