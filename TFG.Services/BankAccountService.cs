using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class BankAccountService(BankContext bankContext, IMemoryCache cache)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<BankAccountResponseDto>> GetBankAccounts(int pageNumber, int pageSize, string orderBy,
        bool descending)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(BankAccountResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var cacheKey = $"GetBankAccounts-{pageNumber}-{pageSize}-{orderBy}-{descending}";
        if (cache.TryGetValue(cacheKey, out Pagination<BankAccountResponseDto>? bankAccounts))
        {
            if (bankAccounts != null) return bankAccounts;
        }

        var bankAccountsQuery = bankContext.BankAccounts.Include(ba => ba.UsersId).Where(ba => !ba.IsDeleted);
        var paginatedBankAccounts = await Pagination<BankAccount>.CreateAsync(bankAccountsQuery, pageNumber, pageSize, orderBy, descending);
        bankAccounts = new Pagination<BankAccountResponseDto>(_mapper.Map<List<BankAccountResponseDto>>(paginatedBankAccounts.Items),
            paginatedBankAccounts.TotalCount, pageNumber, pageSize);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, bankAccounts, cacheEntryOptions);

        return bankAccounts;
    }

    public async Task<BankAccountResponseDto> GetBankAccount(Guid id)
    {
        var cacheKey = $"GetBankAccount-{id}";
        if (cache.TryGetValue(cacheKey, out BankAccountResponseDto? bankAccount))
        {
            if (bankAccount != null) return bankAccount;
        }

        var bankAccountEntity =
            await bankContext.BankAccounts.Include(ba => ba.UsersId).FirstOrDefaultAsync(ba => ba.Id == id) ??
            throw new HttpException(404, "BankAccount not found");
        bankAccount = _mapper.Map<BankAccountResponseDto>(bankAccountEntity);
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, bankAccount, cacheEntryOptions);

        return bankAccount ?? throw new HttpException(404, "BankAccount not found");
    }

    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId(Guid userId)
    {
        var cacheKey = $"GetBankAccountsByUserId-{userId}";
        if (cache.TryGetValue(cacheKey, out List<BankAccountResponseDto>? bankAccounts))
        {
            return bankAccounts ?? throw new HttpException(404, "BankAccounts not found");
        }

        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        bankAccounts = bankAccountList.Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, bankAccounts, cacheEntryOptions);

        return bankAccounts ?? throw new HttpException(404, "BankAccounts not found");
    }

    private static void IsValid(BankAccountCreateDto bankAccountCreateDto)
    {
        if (!Enum.TryParse(typeof(AccountType), bankAccountCreateDto.AccountType, out _))
        {
            throw new HttpException(400,
                "Invalid account type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(AccountType))));
        }
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(Guid bankAccountId)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountId) ??
            throw new HttpException(404, "Bank account not found");
        
        var transactions = await bankContext.Transactions
            .Where(t => t.IdAccountOrigin == bankAccount.Id || t.IdAccountDestination == bankAccount.Id)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetExpensesForAccount(Guid bankAccountId)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountId) ??
                          throw new HttpException(404, "Bank account not found");
        
        var transactions = await bankContext.Transactions
            .Where(t => t.IdAccountOrigin == bankAccount.Id)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }
    
    public async Task<List<TransactionResponseDto>> GetIncomesForAccount(Guid bankAccountId)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountId) ??
                          throw new HttpException(404, "Bank account not found");
        
        var transactions = await bankContext.Transactions
            .Where(t => t.IdAccountDestination == bankAccount.Id)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }
    
    public async Task<BankAccountResponseDto> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        IsValid(bankAccountCreateDto);
        var users = await bankContext.Users.Where(u => bankAccountCreateDto.UsersId.Contains(u.Id)).ToListAsync();

        if (users.Count != bankAccountCreateDto.UsersId.Count)
        {
            throw new HttpException(404, "Users not found");
        }

        var bankAccount = _mapper.Map<BankAccount>(bankAccountCreateDto);
        bankAccount.UsersId = users;
        foreach (var user in users)
        {
            user.BankAccounts.Add(bankAccount);
        }

        bankContext.BankAccounts.Add(bankAccount);
        await bankContext.SaveChangesAsync();

        var bankAccountResponseDto = _mapper.Map<BankAccountResponseDto>(bankAccount);

        await ClearCache();
        
        return bankAccountResponseDto;
    }

    public async Task<BankAccountResponseDto> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate =
            await bankContext.BankAccounts.Include(ba => ba.UsersId).FirstOrDefaultAsync(ba => ba.Id == id) ??
            throw new HttpException(404, "Bank account not found");

        if (bankAccount.UsersId != null)
        {
            var users = await bankContext.Users.Where(u => bankAccount.UsersId.Contains(u.Id)).ToListAsync();
            if (users.Count != bankAccount.UsersId.Count)
            {
                throw new HttpException(404, "Users not found");
            }

            foreach (var user in users)
            {
                bankAccountToUpdate.UsersId.Add(user);
                user.BankAccounts.Add(bankAccountToUpdate);
            }

            await bankContext.SaveChangesAsync();

            return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
        }

        _mapper.Map(bankAccount, bankAccountToUpdate);

        await bankContext.SaveChangesAsync();

        await ClearCache();

        return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
    }

    public async Task DeleteBankAccount(Guid id)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            throw new HttpException(404, "Bank account not found");
        }

        bankAccount.IsDeleted = true;
        await bankContext.SaveChangesAsync();

        await ClearCache();
    }
    
    private async Task ClearCache()
    {
        var ids = await bankContext.BankAccounts.Select(ba => ba.Id).ToListAsync();
        cache.Remove("GetBankAccounts-1-10-Id-false");
        foreach (var id in ids)
        {
            cache.Remove("GetBankAccount-" + id);
        }
    }
}