using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsController(TransactionService transactionService) : ControllerBase
{
    [HttpGet()]
    public async Task<ActionResult<List<TransactionResponseDto>>> GetTransactions()
    {
        return await transactionService.GetTransactions();
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