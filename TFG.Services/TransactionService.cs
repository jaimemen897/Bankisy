using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.Hub;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class TransactionService(BankContext bankContext, IMemoryCache cache)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    private readonly IHubContext<MyHub> _hubContext;

    public async Task<Pagination<TransactionResponseDto>> GetTransactions(int pageNumber, int pageSize, string orderBy,
        bool descending, string? search = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var transactionsQuery = bankContext.Transactions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            transactionsQuery = transactionsQuery.Where(t => t.IbanAccountOrigin.Contains(search) ||
                                                             t.IbanAccountDestination.Contains(search) ||
                                                             t.Concept.Contains(search) ||
                                                             t.Date.ToString().Contains(search) ||
                                                             t.Amount.ToString().Contains(search));
        }

        var paginatedTransactions = await transactionsQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            transaction => _mapper.Map<TransactionResponseDto>(transaction));

        return paginatedTransactions;
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
        var account = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IbanAccountOrigin) ??
                      throw new HttpException(404, "Account origin not found");

        var accountDestination =
            await bankContext.BankAccounts.FindAsync(transactionCreateDto.IbanAccountDestination) ??
            throw new HttpException(404, "Account destination not found");

        ValidateTransaction(account, accountDestination, transactionCreateDto);

        var transactionDto = await CreateTransactionPay(account, accountDestination, transactionCreateDto);

        await ClearCache();

        return transactionDto;
    }

    public async Task<BizumResponseDto> CreateBizum(BizumCreateDto bizumCreateDto, Guid userId)
    {
        var user = await bankContext.Users.FindAsync(userId) ?? throw new HttpException(404, "User not found");

        var account = await bankContext.BankAccounts.FirstOrDefaultAsync(b =>
                          b.Users.Any(u => u.Id == user.Id) && b.AcceptBizum && !b.IsDeleted) ??
                      throw new HttpException(404, "Account origin not found or not accepting Bizum");

        var userDestination =
            await bankContext.Users.FirstOrDefaultAsync(u => u.Phone == bizumCreateDto.PhoneNumberUserDestination) ??
            throw new HttpException(404, "User destination not found");

        var accountDestination =
            await bankContext.BankAccounts.FirstOrDefaultAsync(b =>
                b.Users.Any(u => u.Id == userDestination.Id) && b.AcceptBizum && !b.IsDeleted) ??
            throw new HttpException(404, "Account destination not found or not accepting Bizum");

        var transactionCreate = new TransactionCreateDto
        {
            Amount = bizumCreateDto.Amount,
            Concept = bizumCreateDto.Concept,
            IbanAccountOrigin = account.Iban,
            IbanAccountDestination = accountDestination.Iban
        };

        ValidateBizum(user, userDestination, account, bizumCreateDto);

        await CreateTransactionPay(account, accountDestination, transactionCreate);

        await ClearCache();

        return _mapper.Map<BizumResponseDto>(bizumCreateDto);
    }

    private async Task<TransactionResponseDto> CreateTransactionPay(BankAccount accountOrigin,
        BankAccount accountDestination, TransactionCreateDto transactionCreateDto)
    {
        var transaction = _mapper.Map<Transaction>(transactionCreateDto);
        accountOrigin.TransactionsOrigin.Add(transaction);
        accountDestination.TransactionsDestination.Add(transaction);
        bankContext.Transactions.Add(transaction);

        accountOrigin.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;

        await bankContext.SaveChangesAsync();

        var recipientUsername = accountDestination.Users.FirstOrDefault()?.Username ?? "";

        await _hubContext.Clients.User(recipientUsername).SendAsync("ReceiveMessage", "Transferencia recibida",
            $"Se ha recibido una transferencia de {transaction.Amount}€");

        return _mapper.Map<TransactionResponseDto>(transaction);
    }

    private static void ValidateBizum(User user, User userDestination, BankAccount accountorigin,
        BizumCreateDto bizumCreateDto)
    {
        if (user.Id == userDestination.Id)
        {
            throw new HttpException(400, "Origin and destination users cannot be the same");
        }

        if (accountorigin.Balance < bizumCreateDto.Amount)
        {
            throw new HttpException(400, "Insufficient funds in the origin account");
        }

        if (bizumCreateDto.Amount <= 0)
        {
            throw new HttpException(400, "Transaction amount must be greater than zero");
        }
    }

    private static void ValidateTransaction(BankAccount accountOrigin, BankAccount accountDestination,
        TransactionCreateDto transactionCreateDto)
    {
        if (accountOrigin.Iban == accountDestination.Iban)
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

    public async Task AddPaymentIntent(IncomeCreateDto incomeCreateDto)
    {
        var account = await bankContext.BankAccounts.FindAsync(incomeCreateDto.IbanAccountDestination) ??
                      throw new HttpException(404, "Account origin not found");

        var transaction = new Transaction
        {
            Amount = incomeCreateDto.Amount,
            Concept = "Ingreso por Stripe",
            Date = DateTime.UtcNow,
            IbanAccountOrigin = null,
            IbanAccountDestination = account.Iban
        };

        account.TransactionsOrigin.Add(transaction);
        account.Balance += transaction.Amount;

        bankContext.Transactions.Add(transaction);
        await bankContext.SaveChangesAsync();
    }

    private async Task ClearCache()
    {
        var ids = await bankContext.Transactions.Select(t => t.Id).ToListAsync();
        foreach (var id in ids)
        {
            cache.Remove("GetTransaction-" + id);
        }
    }
}