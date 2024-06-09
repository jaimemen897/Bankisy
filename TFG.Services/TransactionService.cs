using System.Linq.Expressions;
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
using TFG.Services.Mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class TransactionService(
    BankContext bankContext,
    IMemoryCache cache,
    IHubContext<MyHub> hubContext,
    BankAccountService bankAccountService)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    private readonly List<int> _transactionIds = [];

    //GET
    public async Task<Pagination<TransactionResponseDto>> GetTransactions(int pageNumber, int pageSize, string orderBy,
        bool descending, Guid? userId = null, string? search = null, string? filter = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
            throw new HttpException(400, "Invalid orderBy parameter");

        var transactionsQuery = bankContext.Transactions.Where(t => t.Id != 0);

        if (!string.IsNullOrWhiteSpace(search))
            transactionsQuery = transactionsQuery.Where(t =>
                (t.IbanAccountOrigin != null && t.IbanAccountOrigin.Contains(search)) ||
                t.IbanAccountDestination.Contains(search) ||
                t.Concept.Contains(search) || t.Amount.ToString().Contains(search));

        if (userId != null)
        {
            var bankAccountIbans = await bankContext.BankAccounts
                .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == userId))
                .Select(account => account.Iban)
                .ToListAsync();

            transactionsQuery = transactionsQuery
                .Where(t => bankAccountIbans.Contains(t.IbanAccountOrigin) ||
                            bankAccountIbans.Contains(t.IbanAccountDestination));
        }

        if (!string.IsNullOrEmpty(filter))
        {
            var date = DateTime.Parse(filter);
            date = date.ToUniversalTime();
            transactionsQuery = transactionsQuery.Where(t => t.Date.Date >= date.Date);
        }

        var paginatedTransactions = await transactionsQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            _mapper.Map<TransactionResponseDto>);

        return paginatedTransactions;
    }

    public async Task<TransactionResponseDto> GetTransaction(int id)
    {
        var cacheKey = $"GetTransaction-{id}";
        if (cache.TryGetValue(cacheKey, out TransactionResponseDto? transaction))
            if (transaction != null)
                return transaction;

        var transactionEntity = await bankContext.Transactions.FindAsync(id) ??
                                throw new HttpException(404, "Transaction not found");
        transaction = _mapper.Map<TransactionResponseDto>(transactionEntity);
        AddTransactionToCache(transactionEntity);

        return transaction ?? throw new HttpException(404, "Transaction not found");
    }

    private async Task<List<TransactionResponseDto>> GetTransactions(Expression<Func<Transaction, bool>> filter)
    {
        var transactions = await bankContext.Transactions.Where(filter).OrderByDescending(t => t.Date).ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsByIban(string iban, Guid? userId = null)
    {
        var bankAccount = await bankAccountService.GetBankAccount(iban);

        if (userId != null && bankAccount.UsersId.All(id => id != userId))
            throw new HttpException(403, "You are not the owner of the account");

        return await GetTransactions(t =>
            t.IbanAccountOrigin == bankAccount.Iban || t.IbanAccountDestination == bankAccount.Iban);
    }

    public async Task<UserSummary> GetSummary(Guid userId)
    {
        var bankAccounts = await bankContext.BankAccounts
            .Where(b => b.Users.Any(u => u.Id == userId))
            .Select(b => b.Iban)
            .ToListAsync();

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToUniversalTime();

        var transactionsFromFirstOfMonth = await GetTransactions(t =>
            bankAccounts.Contains(t.IbanAccountDestination) && t.Date >= firstDayOfMonth);
        var expensesFromFirstOfMonth = await GetTransactions(t =>
            bankAccounts.Contains(t.IbanAccountOrigin ?? "") && t.Date >= firstDayOfMonth);

        var totalBalance = bankAccounts.Sum(iban => bankContext.BankAccounts.First(b => b.Iban == iban).Balance);
        var totalIncomes = transactionsFromFirstOfMonth.Sum(t => t.Amount);
        var totalExpenses = expensesFromFirstOfMonth.Sum(t => t.Amount);

        return new UserSummary
        {
            TotalBalance = totalBalance,
            TotalIncomes = totalIncomes,
            TotalExpenses = totalExpenses
        };
    }

    //CREATE
    public async Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto transactionCreateDto, Guid userId)
    {
        var account = await bankContext.BankAccounts.Include(b => b.Users)
                          .FirstOrDefaultAsync(b => b.Iban == transactionCreateDto.IbanAccountOrigin) ??
                      throw new HttpException(404, "Account origin not found");

        var accountDestination =
            await bankContext.BankAccounts.Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Iban == transactionCreateDto.IbanAccountDestination) ??
            throw new HttpException(404, "Account destination not found");

        ValidateTransaction(account, accountDestination, transactionCreateDto, userId);

        var transactionDto = await CreateTransactionPay(account, accountDestination, transactionCreateDto);

        ClearCache();

        return transactionDto;
    }

    public async Task<BizumResponseDto> CreateBizum(BizumCreateDto bizumCreateDto, Guid userId)
    {
        var user = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ??
                   throw new HttpException(404, "User not found");

        var account = await bankContext.BankAccounts.Include(b => b.Users).FirstOrDefaultAsync(b =>
                          b.Users.Any(u => u.Id == user.Id) && b.AcceptBizum && !b.IsDeleted) ??
                      throw new HttpException(404, "Account origin not found or not accepting Bizum");

        var userDestination =
            await bankContext.Users.FirstOrDefaultAsync(u => u.Phone == bizumCreateDto.PhoneNumberUserDestination) ??
            throw new HttpException(404, "User destination not found");

        var accountDestination =
            await bankContext.BankAccounts.Include(b => b.Users).FirstOrDefaultAsync(b =>
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

        await CreateTransactionPay(account, accountDestination, transactionCreate, true);

        ClearCache();

        return _mapper.Map<BizumResponseDto>(bizumCreateDto);
    }

    private async Task<TransactionResponseDto> CreateTransactionPay(BankAccount accountOrigin,
        BankAccount accountDestination, TransactionCreateDto transactionCreateDto, bool isBizum = false)
    {
        var transaction = _mapper.Map<Transaction>(transactionCreateDto);

        // Add the transaction to the origin and destination accounts
        accountOrigin.TransactionsOrigin.Add(transaction);
        accountDestination.TransactionsDestination.Add(transaction);

        // Update the balances
        accountOrigin.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;

        // Save the changes
        await bankContext.SaveChangesAsync();

        var recipientUser = accountDestination.Users.FirstOrDefault();
        var message = isBizum
            ? $"Se ha recibido un Bizum de {transaction.Amount}€ con concepto: {transaction.Concept}"
            : $"Se ha recibido una transferencia de {transaction.Amount}€ con concepto: {transaction.Concept}";
        if (recipientUser != null)
            if (MyHub._userConnections.TryGetValue(recipientUser.Id.ToString(), out var connectionId))
                await hubContext.Clients.Client(connectionId).SendAsync("TransferReceived", recipientUser.Id, message);

        return _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task AddPaymentIntent(IncomeCreateDto incomeCreateDto, Guid userId)
    {
        var userAsync = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ??
                        throw new HttpException(404, "User not found");

        var account = await bankContext.BankAccounts.FirstOrDefaultAsync(b =>
                          b.Users.Any(u => u.Id == userAsync.Id) && !b.IsDeleted) ??
                      throw new HttpException(404, "Account origin not found");

        var transaction = new Transaction
        {
            Amount = incomeCreateDto.Amount,
            Concept = "Ingreso por Stripe",
            Date = DateTime.UtcNow,
            IbanAccountOrigin = null,
            IbanAccountDestination = account.Iban
        };

        account.TransactionsDestination.Add(transaction);
        account.Balance += transaction.Amount;

        //bankContext.Transactions.Add(transaction);
        await bankContext.SaveChangesAsync();
    }

    //VALIDATE
    private static void ValidateBizum(User user, User userDestination, BankAccount accountOrigin,
        BizumCreateDto bizumCreateDto)
    {
        if (user.Id == userDestination.Id)
            throw new HttpException(400, "Origin and destination users cannot be the same");

        if (accountOrigin.Balance < bizumCreateDto.Amount)
            throw new HttpException(400, "Insufficient funds in the origin account");

        if (bizumCreateDto.Amount <= 0) throw new HttpException(400, "Transaction amount must be greater than zero");
    }

    private static void ValidateTransaction(BankAccount accountOrigin, BankAccount accountDestination,
        TransactionCreateDto transactionCreateDto, Guid? userId = null)
    {
        if (accountOrigin.Iban == accountDestination.Iban)
            throw new HttpException(400, "Origin and destination accounts cannot be the same");

        if (accountOrigin.Balance < transactionCreateDto.Amount)
            throw new HttpException(400, "Insufficient funds in the origin account");

        if (transactionCreateDto.Amount <= 0)
            throw new HttpException(400, "Transaction amount must be greater than zero");

        if (accountOrigin.Users.All(u => u.Id != userId))
            throw new HttpException(403, "You are not the owner of the account");
    }

    //DELETE
    public async Task DeleteTransaction(int id)
    {
        var transaction = await bankContext.Transactions.FindAsync(id);
        if (transaction == null) throw new HttpException(404, "Transaction not found");

        bankContext.Transactions.Remove(transaction);
        await bankContext.SaveChangesAsync();

        ClearCache();
    }

    //CACHE
    private void AddTransactionToCache(Transaction transaction)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        _transactionIds.Add(transaction.Id);
        cache.Set("GetTransaction-" + transaction.Id, _mapper.Map<TransactionResponseDto>(transaction),
            cacheEntryOptions);
    }

    private void ClearCache()
    {
        foreach (var cacheKey in _transactionIds.Select(id => "GetTransaction-" + id)) cache.Remove(cacheKey);
    }
}