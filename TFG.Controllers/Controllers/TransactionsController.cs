using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.transactions;
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
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, null, search,
            filter);
    }

    [Authorize(Policy = "User")]
    [HttpGet("myself")]
    public async Task<ActionResult<Pagination<TransactionResponseDto>>> GetMyTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, GetUserId(), search,
            filter);
    }

    [Authorize(Policy = "User")]
    [HttpGet("summary")]
    public async Task<UserSummary> GetSummary()
    {
        return await transactionService.GetSummary(GetUserId());
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
        return await transactionService.GetTransactionsByIban(bankAccountIban);
    }

    [Authorize(Policy = "User")]
    [HttpGet("bankaccount/{iban}")]
    public async Task<List<TransactionResponseDto>> GetTransactionsByIban(string iban)
    {
        return await transactionService.GetTransactionsByIban(iban, GetUserId());
    }

    //CREATE
    [Authorize(Policy = "User")]
    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return Created("", await transactionService.CreateTransaction(transaction, GetUserId()));
    }

    [Authorize(Policy = "User")]
    [HttpPost("bizum")]
    public async Task<ActionResult<BizumResponseDto>> CreateBizum(BizumCreateDto transaction)
    {
        return Created("", await transactionService.CreateBizum(transaction, GetUserId()));
    }

    //DELETE
    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        await transactionService.DeleteTransaction(id);
        return NoContent();
    }

    private Guid GetUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HttpException(401, "Unauthorized"));
    }
}