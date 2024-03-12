using AutoMapper;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class TransactionService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<TransactionResponseDto>> GetTransactions(int pageNumber, int pageSize, string orderBy, bool descending)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var transactions = bankContext.Transactions;

        var paginatedTransactions =
            await Pagination<Transaction>.CreateAsync(transactions, pageNumber, pageSize, orderBy, descending);
        
        var transactionsMapped = _mapper.Map<List<TransactionResponseDto>>(paginatedTransactions);
        
        return new Pagination<TransactionResponseDto>(transactionsMapped, paginatedTransactions.TotalCount, pageNumber,
            pageSize);
    }

    public async Task<TransactionResponseDto> GetTransaction(int id)
    {
        var transaction = await bankContext.Transactions.FindAsync(id) ?? throw new HttpException(404, "Transaction not found");
        return _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto transactionCreateDto)
    {
        var account = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountOrigin) ?? throw new HttpException(404, "Account not found");
        var accountDestination = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountDestination) ?? throw new HttpException(404, "Account not found");

        if (account.Id == accountDestination.Id)
        {
            throw new HttpException(400, "Origin and destination accounts cannot be the same");
        }

        if (account.Balance < transactionCreateDto.Amount)
        {
            throw new HttpException(400, "Insufficient funds in the origin account");
        }

        if (transactionCreateDto.Amount <= 0)
        {
            throw new HttpException(400, "Transaction amount must be greater than zero");
        }

        var transaction = _mapper.Map<Transaction>(transactionCreateDto);

        account.Transactions.Add(transaction);
        bankContext.Transactions.Add(transaction);

        account.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;

        await bankContext.SaveChangesAsync();

        var transactionResponseDto = _mapper.Map<TransactionResponseDto>(transaction);
        return transactionResponseDto;
    }
    
    public async Task DeleteTransaction(int id)
    {
        var transaction = await bankContext.Transactions.FindAsync(id);
        if (transaction == null)
        {
            throw new HttpException(404, "Transaction not found");
        }

        bankContext.Transactions.Remove(transaction);
        await bankContext.SaveChangesAsync();
    }
}