using AutoMapper;
using IbanNet.Registry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class BankAccountService(BankContext bankContext, IMemoryCache cache)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<BankAccountResponseDto>> GetBankAccounts(int pageNumber, int pageSize, string orderBy,
        bool descending, string? search = null, string? filter = null, bool? isDeleted = false)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(BankAccountResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase) &&
                          orderBy.ToLower() != "usersname"))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var bankAccountsQuery = bankContext.BankAccounts.Include(ba => ba.Users).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var userNames = search.Split(',');
            bankAccountsQuery = bankAccountsQuery.Where(ba => ba.Balance.ToString().Contains(search) ||
                                                              ba.Iban.Contains(search) ||
                                                              ba.Users.Any(u => userNames.Contains(u.Name)));
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            if (Enum.TryParse<AccountType>(filter, out var accountTypeFilter))
            {
                bankAccountsQuery = bankAccountsQuery.Where(ba => ba.AccountType == accountTypeFilter);
            }
        }

        if (isDeleted != null)
        {
            bankAccountsQuery = bankAccountsQuery.Where(ba => ba.IsDeleted == isDeleted);
        }

        var paginatedBankAccounts = await bankAccountsQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            bankAccount => _mapper.Map<BankAccountResponseDto>(bankAccount));

        return paginatedBankAccounts;
    }

    public async Task<BankAccountResponseDto> GetBankAccount(string iban)
    {
        /*var cacheKey = $"GetBankAccount-{iban}";
        if (cache.TryGetValue(cacheKey, out BankAccountResponseDto? bankAccount))
        {
            if (bankAccount != null) return bankAccount;
        }*/

        var bankAccountEntity =
            await bankContext.BankAccounts.Include(ba => ba.Users).FirstOrDefaultAsync(ba => ba.Iban == iban) ??
            throw new HttpException(404, "BankAccount not found");

        var bankAccount = _mapper.Map<BankAccountResponseDto>(bankAccountEntity);
        /*var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        cache.Set(cacheKey, bankAccount, cacheEntryOptions);*/

        return bankAccount ?? throw new HttpException(404, "BankAccount not found");
    }

    private static void IsValid(BankAccountCreateDto bankAccountCreateDto)
    {
        if (!Enum.TryParse(typeof(AccountType), bankAccountCreateDto.AccountType, out _))
        {
            throw new HttpException(400,
                "Invalid account type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(AccountType))));
        }
    }

    private static void IsValid(BankAccountUpdateDto bankAccountUpdateDto)
    {
        if (!Enum.TryParse(typeof(AccountType), bankAccountUpdateDto.AccountType, out _))
        {
            throw new HttpException(400,
                "Invalid account type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(AccountType))));
        }
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(string bankAccountIban)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountIban) ??
                          throw new HttpException(404, "Bank account not found");

        var transactions = await bankContext.Transactions
            .Where(t => t.IbanAccountOrigin == bankAccount.Iban || t.IbanAccountDestination == bankAccount.Iban)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetExpensesForAccount(string bankAccountIban)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountIban) ??
                          throw new HttpException(404, "Bank account not found");

        var transactions = await bankContext.Transactions
            .Where(t => t.IbanAccountOrigin == bankAccount.Iban)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetIncomesForAccount(string bankAccountIban)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(bankAccountIban) ??
                          throw new HttpException(404, "Bank account not found");

        var transactions = await bankContext.Transactions
            .Where(t => t.IbanAccountDestination == bankAccount.Iban)
            .ToListAsync();

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId(Guid userId)
    {
        var bankAccounts = await bankContext.BankAccounts
            .Include(ba => ba.Users)
            .Where(ba => ba.Users.Any(u => u.Id == userId) && !ba.IsDeleted)
            .ToListAsync();

        return bankAccounts.Select(bankAccount => _mapper.Map<BankAccountResponseDto>(bankAccount)).ToList();
    }

    public async Task<BankAccountResponseDto> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        IsValid(bankAccountCreateDto);
        var users = await bankContext.Users.Where(u => bankAccountCreateDto.UsersId.Contains(u.Id)).ToListAsync();
        bankAccountCreateDto.UsersId = bankAccountCreateDto.UsersId.Distinct().ToList();
        if (users.Count != bankAccountCreateDto.UsersId.Count)
        {
            throw new HttpException(404, "Users not found");
        }

        var ibanGenerator = new IbanGenerator();
        var iban = ibanGenerator.Generate("ES");

        var bankAccount = _mapper.Map<BankAccount>(bankAccountCreateDto);
        bankAccount.Iban = iban.ToString();
        bankAccount.Users = users;
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

    public async Task<BankAccountResponseDto> UpdateBankAccount(string iban, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate =
            await bankContext.BankAccounts.Include(ba => ba.Users).FirstOrDefaultAsync(ba => ba.Iban == iban) ??
            throw new HttpException(404, "Bank account not found");

        IsValid(bankAccount);

        if (bankAccount.UsersId != null)
        {
            var users = await bankContext.Users.Where(u => bankAccount.UsersId.Contains(u.Id)).ToListAsync();
            /*eliminar repetidos*/
            bankAccount.UsersId = bankAccount.UsersId.Distinct().ToList();

            if (users.Count != bankAccount.UsersId.Count)
            {
                throw new HttpException(404, "Users not found");
            }

            bankAccountToUpdate.Users.Clear();
            foreach (var user in users.Where(user => bankAccountToUpdate.Users.All(u => u.Id != user.Id)))
            {
                bankAccountToUpdate.Users.Add(user);
                user.BankAccounts.Add(bankAccountToUpdate);
            }

            if (bankAccount.AccountType != null)
            {
                bankAccountToUpdate.AccountType = Enum.Parse<AccountType>(bankAccount.AccountType);
            }


            await bankContext.SaveChangesAsync();

            return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
        }

        _mapper.Map(bankAccount, bankAccountToUpdate);

        await bankContext.SaveChangesAsync();

        await ClearCache();

        return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
    }

    public async Task DeleteBankAccount(string iban)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(iban);
        if (bankAccount == null)
        {
            throw new HttpException(404, "Bank account not found");
        }

        bankAccount.IsDeleted = true;
        await bankContext.SaveChangesAsync();

        await ClearCache();
    }

    public async Task ActivateBankAccount(string iban)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(iban);
        if (bankAccount == null)
        {
            throw new HttpException(404, "Bank account not found");
        }

        bankAccount.IsDeleted = false;
        await bankContext.SaveChangesAsync();

        await ClearCache();
    }

    public async Task ActiveBizum(string iban, Guid userId)
    {
        var bankAccount = await bankContext.BankAccounts.Include(ba => ba.Users)
                              .FirstOrDefaultAsync(ba => ba.Iban == iban && !ba.IsDeleted) ??
                          throw new HttpException(404, "Bank account not found");

        var bankAccounts = await bankContext.BankAccounts.Include(ba => ba.Users)
            .Where(ba => ba.Users.Any(u => u.Id == userId) && !ba.IsDeleted)
            .ToListAsync();

        _ = await bankContext.Users.FindAsync(userId) ?? throw new HttpException(404, "User not found");

        if (bankAccounts.All(ba => ba.Users.All(u => u.Id != userId)))
        {
            throw new HttpException(404, "User not found in bank account");
        }

        bankAccounts.ForEach(ba => ba.AcceptBizum = false);
        bankAccount.AcceptBizum = true;
        await bankContext.SaveChangesAsync();
        await ClearCache();
    }
    
    internal async Task<BankAccountResponseDto> GetPrincipalAccount(Guid userId)
    {
        var bankAccount = await bankContext.BankAccounts.Include(ba => ba.Users)
            .FirstOrDefaultAsync(ba => ba.Users.Any(u => u.Id == userId) && ba.AcceptBizum && !ba.IsDeleted) ??
                          throw new HttpException(404, "Bank account not found");

        return _mapper.Map<BankAccountResponseDto>(bankAccount);
    }

    private async Task ClearCache()
    {
        var ibans = await bankContext.BankAccounts.Select(ba => ba.Iban).ToListAsync();
        /*cache.Remove("GetBankAccounts-1-10-Id-false");*/
        foreach (var iban in ibans)
        {
            cache.Remove("GetBankAccount-" + iban);
        }
    }
}