using Microsoft.AspNetCore.Http;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Services.Exceptions;

namespace TFG.Services;

public class IndexService(
    BankAccountService bankAccountService,
    TransactionService transactionService,
    SessionService sessionService,
    UsersService usersService)
{
    //PROFILE
    public async Task<UserResponseDto> UploadAvatar(IFormFile file, string host)
    {
        var user = sessionService.GetMyself().Result;
        return await usersService.UploadAvatar(user.Id, file, host);
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
}