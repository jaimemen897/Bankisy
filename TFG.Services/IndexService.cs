using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class IndexService(
    BankAccountService bankAccountService,
    TransactionService transactionService,
    CardService cardService,
    SessionService sessionService,
    UsersService usersService)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    private readonly UserResponseDto user = sessionService.GetMyself().Result;

    public UserResponseDto GetMyself()
    {
        return sessionService.GetMyself().Result;
    }

    //BANK ACCOUNTS
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId()
    {
        return await bankAccountService.GetBankAccountsByUserId(user.Id);
    }

    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    public async Task ActiveBizum(string iban)
    {
        await bankAccountService.ActiveBizum(iban, user.Id);
    }

    public async Task<decimal> GetTotalBalanceByUserId()
    {
        return await bankAccountService.GetTotalBalanceByUserId(user.Id);
    }

    //TRANSACTIONS
    public async Task<Pagination<TransactionResponseDto>> GetTransactionsByUserId(int pageNumber, int pageSize,
        string orderBy, bool descending, string? search = null, string? filter = null)
    {
        return await transactionService.GetTransactions(pageNumber, pageSize, orderBy, descending, user, search,
            filter);
    }

    public async Task<List<TransactionResponseDto>> GetExpensesByUserId()
    {
        return await transactionService.GetExpensesByUserId(user.Id);
    }

    public async Task<List<TransactionResponseDto>> GetIncomesByUserId()
    {
        return await transactionService.GetIncomesByUserId(user.Id);
    }

    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        var bankAccount = await bankAccountService.GetBankAccount(transaction.IbanAccountOrigin);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");

        return await transactionService.CreateTransaction(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsByIban(string iban)
    {
        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");

        return await transactionService.GetTransactionsForAccount(iban);
    }

    public async Task<BizumResponseDto> CreateBizum(BizumCreateDto bizumCreateDto)
    {
        return await transactionService.CreateBizum(bizumCreateDto, user.Id);
    }

    //CARDS
    public async Task<ActionResult<CardResponseDto>> GetCardByCardNumber(string cardNumber)
    {
        var card = await cardService.GetCardByCardNumber(cardNumber);
        if (card.User.Id != user.Id) throw new HttpException(403, "You are not the owner of the card");

        return await cardService.GetCardByCardNumber(cardNumber);
    }

    public async Task<ActionResult<CardResponseDto>> CreateCard(CardCreateDto cardCreateDto)
    {
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
        return await cardService.GetCardsByUserId(user.Id);
    }

    public async Task<List<CardResponseDto>> GetCardsByIban(string iban)
    {
        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != user.Id))
            throw new HttpException(403, "You are not the owner of the account");
        return await cardService.GetCardsByIban(iban);
    }

    //PROFILE
    public async Task<string> UpdateProfile(UserUpdateDto userUpdateDto)
    {
        var userUpdated = await usersService.UpdateUser(user.Id, userUpdateDto);
        var userMapped = _mapper.Map<User>(userUpdated);
        return sessionService.GetToken(userMapped);
    }

    public async Task<UserResponseDto> UploadAvatar(IFormFile file, string host)
    {
        return await usersService.UploadAvatar(user.Id, file, host);
    }

    public async Task DeleteAvatar()
    {
        await usersService.DeleteAvatar(user.Id);
    }

    //PAYMENT INTENT
    public async Task AddPaymentIntent(decimal ammount, Guid userId, string iban)
    {
        var userAsync = await usersService.GetUserAsync(userId);

        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != userAsync.Id))
            throw new HttpException(403, "You are not the owner of the account");

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
        var card = await cardService.GetCardByCardNumber(cardNumber);

        if (card.User.Id != user.Id) throw new HttpException(403, "You are not the owner of the card");
    }
}