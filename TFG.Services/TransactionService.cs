using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class TransactionService(BankContext bankContext, IMemoryCache cache)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<TransactionResponseDto>> GetTransactions(int pageNumber, int pageSize, string orderBy,
        bool descending)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var cacheKey = $"GetTransactions-{pageNumber}-{pageSize}-{orderBy}-{descending}";
        if (cache.TryGetValue(cacheKey, out Pagination<TransactionResponseDto>? transactions))
        {
            if (transactions != null) return transactions;
        }

        var transactionsQuery = bankContext.Transactions;
        var paginatedTransactions = await Pagination<Transaction>.CreateAsync(transactionsQuery, pageNumber, pageSize, orderBy, descending);
        transactions = new Pagination<TransactionResponseDto>(
            _mapper.Map<List<TransactionResponseDto>>(paginatedTransactions.Items), paginatedTransactions.TotalCount,
            pageNumber, pageSize);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, transactions, cacheEntryOptions);

        return transactions;
    }

    public async Task<TransactionResponseDto> GetTransaction(int id)
    {
        var cacheKey = $"GetTransaction-{id}";
        if (cache.TryGetValue(cacheKey, out TransactionResponseDto? transaction))
        {
            if (transaction != null) return transaction;
        }

        var transactionEntity = await bankContext.Transactions.FindAsync(id) ??
                                throw new HttpException(404, "Transaction not found");
        transaction = _mapper.Map<TransactionResponseDto>(transactionEntity);
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, transaction, cacheEntryOptions);

        return transaction ?? throw new HttpException(404, "Transaction not found");
    }

    public async Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto transactionCreateDto)
    {
        var account = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountOrigin) ??
                      throw new HttpException(404, "Account origin not found");
        var accountDestination = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountDestination) ??
                                 throw new HttpException(404, "Account destination not found");
        
        ValidateTransaction(account, accountDestination, transactionCreateDto);

        var transactionDto = await CreateTransactionPay(account, accountDestination, transactionCreateDto);

        await ClearCache();

        return transactionDto;
    }

    private static void ValidateTransaction(BankAccount accountOrigin, BankAccount accountDestination, TransactionCreateDto transactionCreateDto) 
    {
        if (accountOrigin.Id == accountDestination.Id)
        {
            throw new HttpException(400, "Origin and destination accounts cannot be the same");
        }

        if (accountOrigin.Balance < transactionCreateDto.Amount)
        {
            throw new HttpException(400, "Insufficient funds in the origin account");
        }

        if (transactionCreateDto.Amount <= 0)
        {
            throw new HttpException(400, "Transaction amount must be greater than zero");
        }
    }
    
    private async Task<TransactionResponseDto> CreateTransactionPay(BankAccount accountOrigin, BankAccount accountDestination, TransactionCreateDto transactionCreateDto)
    {
        var transaction = _mapper.Map<Transaction>(transactionCreateDto);
        accountOrigin.Transactions.Add(transaction);
        bankContext.Transactions.Add(transaction);

        accountOrigin.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;

        await bankContext.SaveChangesAsync();
        return _mapper.Map<TransactionResponseDto>(transaction);
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

        await ClearCache();
    }
    
    private async Task ClearCache()
    {
        var ids = await bankContext.Transactions.Select(t => t.Id).ToListAsync();
        cache.Remove("GetTransactions-1-10-Id-False");
        foreach (var id in ids)
        {
            cache.Remove("GetTransaction-" + id);
        }
    }
}