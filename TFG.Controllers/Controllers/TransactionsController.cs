using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet()]
    public async Task<ActionResult<List<TransactionResponseDto>>> GetTransactions()
    {
        return await _transactionService.GetTransactions();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        return await _transactionService.GetTransaction(id);
    }

    [HttpPost()]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await _transactionService.CreateTransaction(transaction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, TransactionUpdateDto transaction)
    {
        return await _transactionService.UpdateTransaction(id, transaction);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteTransaction(int id)
    {
        return await _transactionService.DeleteTransaction(id);
    }
}