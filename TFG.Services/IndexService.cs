using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class IndexService(
    BankContext bankContext,
    BankAccountService bankAccountService,
    TransactionService transactionService,
    CardService cardService,
    SessionService sessionService,
    UsersService usersService)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public UserResponseDto GetMyself()
    {
        return sessionService.GetMyself().Result;
    }

    //BANK ACCOUNTS
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId()
    {
        var user = await sessionService.GetMyself();

        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.Users).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == user.Id))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        return bankAccounts ?? throw new HttpException(404, "BankAccounts not found");
    }

    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    public async Task ActiveBizum(string iban)
    {
        var user = await sessionService.GetMyself();
        await bankAccountService.ActiveBizum(iban, user.Id);
    }

    //BALANCE
    public async Task<decimal> GetTotalBalanceByUserId()
    {
        var user = await sessionService.GetMyself();
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.Users).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == user.Id))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
        return bankAccounts.Sum(ba => ba.Balance);
    }

    //TRANSACTIONS
    public async Task<Pagination<TransactionResponseDto>> GetTransactionsByUserId(int pageNumber,
        int pageSize, string orderBy, bool descending, string? search = null, string? filter = null)
    {
        var user = await sessionService.GetMyself();

        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
            throw new HttpException(400, "Invalid orderBy parameter");

        var bankAccountIbans = await bankContext.BankAccounts
            .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == user.Id))
            .Select(account => account.Iban)
            .ToListAsync();

        var transactionQuery = bankContext.Transactions
            .Where(t => bankAccountIbans.Contains(t.IbanAccountOrigin) ||
                        bankAccountIbans.Contains(t.IbanAccountDestination));

        if (!string.IsNullOrEmpty(search))
            transactionQuery = transactionQuery.Where(t => t.IbanAccountOrigin.ToLower().Contains(search.ToLower()) ||
                                                           t.IbanAccountDestination.ToLower()
                                                               .Contains(search.ToLower()));

        if (!string.IsNullOrEmpty(filter))
        {
            var date = DateTime.Parse(filter);
            date = date.ToUniversalTime();
            transactionQuery = transactionQuery.Where(t => t.Date.Date >= date.Date);
        }

        var paginatedTransactions = await transactionQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            transaction => _mapper.Map<TransactionResponseDto>(transaction));

        return paginatedTransactions;
    }

    public async Task<List<TransactionResponseDto>> GetExpensesByUserId()
    {
        var user = await sessionService.GetMyself();

        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.Users).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == user.Id))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        var transactions = new List<Transaction>();

        foreach (var bankAccount in bankAccounts)
            transactions.AddRange(await bankContext.Transactions
                .Where(t => bankAccount.Iban == t.IbanAccountOrigin && t.Date.Date.Month == DateTime.UtcNow.Month)
                .ToListAsync());

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetIncomesByUserId()
    {
        var user = await sessionService.GetMyself();
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.Users).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.Users.Any(u => u.Id == user.Id))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        var transactions = new List<Transaction>();

        foreach (var bankAccount in bankAccounts)
            transactions.AddRange(await bankContext.Transactions
                .Where(t => bankAccount.Iban == t.IbanAccountDestination && t.Date.Date.Month == DateTime.UtcNow.Month)
                .ToListAsync());

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        var user = await sessionService.GetMyself();

        var bankAccount = await bankAccountService.GetBankAccount(transaction.IbanAccountOrigin);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");

        return await transactionService.CreateTransaction(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsByIban(string iban)
    {
        var user = await sessionService.GetMyself();
        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");

        return await bankAccountService.GetTransactionsForAccount(iban);
    }

    public async Task<BizumResponseDto> CreateBizum(BizumCreateDto bizumCreateDto)
    {
        var user = await sessionService.GetMyself();
        return await transactionService.CreateBizum(bizumCreateDto, user.Id);
    }

    //CARDS
    public async Task<ActionResult<CardResponseDto>> GetCardByCardNumber(string cardNumber)
    {
        var user = await sessionService.GetMyself();
        var card = await cardService.GetCardByCardNumber(cardNumber);
        if (card.User.Id != user.Id) throw new HttpException(403, "You are not the owner of the card");

        return await cardService.GetCardByCardNumber(cardNumber);
    }

    public async Task<ActionResult<CardResponseDto>> CreateCard(CardCreateDto cardCreateDto)
    {
        var user = await sessionService.GetMyself();

        if (cardCreateDto.UserId != user.Id) throw new HttpException(403, "You are not the owner of the card");

        return await cardService.CreateCard(cardCreateDto);
    }

    public async Task<ActionResult<CardResponseDto>> UpdateCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        await ValidateCardWithUser(cardNumber);

        return await cardService.UpdateCard(cardNumber, cardUpdateDto);
    }

    public async Task DeleteCard(string cardNumber)
    {
        await ValidateCardWithUser(cardNumber);

        await cardService.DeleteCard(cardNumber);
    }

    public async Task<ActionResult<CardResponseDto>> RenovateCard(string cardNumber)
    {
        await ValidateCardWithUser(cardNumber);
        return await cardService.RenovateCard(cardNumber);
    }

    public async Task BlockCard(string cardNumber)
    {
        await ValidateCardWithUser(cardNumber);
        await cardService.BlockCard(cardNumber);
    }

    public async Task UnblockCard(string cardNumber)
    {
        await ValidateCardWithUser(cardNumber);
        await cardService.UnblockCard(cardNumber);
    }

    public async Task ActivateCard(string cardNumber)
    {
        await ValidateCardWithUser(cardNumber);
        await cardService.ActivateCard(cardNumber);
    }

    public async Task<List<CardResponseDto>> GetCardsByUserId()
    {
        var user = await sessionService.GetMyself();
        return await cardService.GetCardsByUserId(user.Id);
    }

    public async Task<List<CardResponseDto>> GetCardsByIban(string iban)
    {
        var user = await sessionService.GetMyself();
        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");
        return await cardService.GetCardsByIban(iban);
    }

    //PROFILE
    public async Task<string> UpdateProfile(UserUpdateDto userUpdateDto)
    {
        var user = await sessionService.GetMyself();
        var userUpdated = await usersService.UpdateUser(user.Id, userUpdateDto);
        var userMapped = _mapper.Map<User>(userUpdated);
        return SessionService.GetToken(userMapped);
    }

    public async Task<UserResponseDto> UploadAvatar(IFormFile file, string host)
    {
        var user = await sessionService.GetMyself();
        return await usersService.UploadAvatar(user.Id, file, host);
    }

    public async Task<UserResponseDto> DeleteAvatar()
    {
        var user = await sessionService.GetMyself();
        return await usersService.DeleteAvatar(user.Id);
    }

    //PAYMENT INTENT
    public async Task AddPaymentIntent(decimal ammount, Guid userId, string iban)
    {
        var user = await usersService.GetUserAsync(userId);

        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != user.Id))
        {
            throw new HttpException(403, "You are not the owner of the account");
        }

        var incomeCreateDto = new IncomeCreateDto
        {
            Amount = ammount,
            IbanAccountDestination = bankAccount.Iban
        };
        await transactionService.AddPaymentIntent(incomeCreateDto);
    }

    //PRIVATE METHODS
    private async Task ValidateCardWithUser(string cardNumber)
    {
        var user = await sessionService.GetMyself();
        var card = await cardService.GetCardByCardNumber(cardNumber);

        if (card.User.Id != user.Id) throw new HttpException(403, "You are not the owner of the card");
    }
}