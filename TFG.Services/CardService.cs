using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.cards;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class CardService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<Pagination<CardResponseDto>> GetCards(int pageNumber, int pageSize, string orderBy,
        bool descending, string? search = null, string? filter = null, bool? isDeleted = null, bool? isBlocked = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(CardResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var cardsQuery = bankContext.Cards.Include(c => c.User).Include(c => c.BankAccount).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            cardsQuery = cardsQuery.Where(c =>
                c.CardNumber.Contains(search) || c.User.Name.Contains(search) || c.BankAccount.Iban.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            if (Enum.TryParse<CardType>(filter, out var cardTypeFilter))
            {
                cardsQuery = cardsQuery.Where(c => c.CardType == cardTypeFilter);
            }
        }
        
        if (isDeleted != null)
        {
            cardsQuery = cardsQuery.Where(c => c.IsDeleted == isDeleted);
        }
        
        if (isBlocked != null)
        {
            cardsQuery = cardsQuery.Where(c => c.IsBlocked == isBlocked);
        }

        var paginatedCards = await cardsQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            card => _mapper.Map<CardResponseDto>(card));

        return paginatedCards;
    }

    public async Task<CardResponseDto> GetCardByCardNumber(string cardNumber)
    {
        var card = await bankContext.Cards.Include(c => c.User).Include(c => c.BankAccount)
            .FirstAsync(c => c.CardNumber == cardNumber) ?? throw new HttpException(404, "Card not found");
        return _mapper.Map<CardResponseDto>(card);
    }
    
    public async Task<List<CardResponseDto>> GetCardsByUserId(Guid userId)
    {
        var cardList = await bankContext.Cards.Include(c => c.User).Include(c => c.BankAccount).ToListAsync();
        var cards = cardList
            .Where(card => !card.IsDeleted && card.UserId == userId)
            .Select(card => _mapper.Map<CardResponseDto>(card)).ToList();

        return cards ?? throw new HttpException(404, "Cards not found");
    }
    
    public async Task<List<CardResponseDto>> GetCardsByIban(string iban)
    {
        var cardList = await bankContext.Cards.Include(c => c.User).Include(c => c.BankAccount).ToListAsync();
        var cards = cardList
            .Where(card => !card.IsDeleted && card.BankAccountIban == iban)
            .Select(card => _mapper.Map<CardResponseDto>(card)).ToList();

        return cards ?? throw new HttpException(404, "Cards not found");
    }

    public async Task<CardResponseDto> CreateCard(CardCreateDto cardCreateDto)
    {
        var user = await bankContext.Users.FindAsync(cardCreateDto.UserId) ??
                   throw new HttpException(404, "User not found");
        var bankAccount = await bankContext.BankAccounts.Include(ba => ba.Users).FirstOrDefaultAsync(ba =>
            ba.Iban == cardCreateDto.BankAccountIban) ?? throw new HttpException(404, "Bank account not found");

        if (!bankAccount.Users.Contains(user))
        {
            throw new HttpException(400, "Bank account does not belong to the user");
        }

        if (await bankContext.Cards.AnyAsync(c => c.BankAccountIban == cardCreateDto.BankAccountIban && !c.IsDeleted))
        {
            throw new HttpException(400, "Bank account already has a card");
        }

        IsValid(cardCreateDto);
        var card = _mapper.Map<Card>(cardCreateDto);
        await bankContext.Cards.AddAsync(card);
        await bankContext.SaveChangesAsync();
        return _mapper.Map<CardResponseDto>(card);
    }

    public async Task<CardResponseDto> UpdateCard(string cardNumber, CardUpdateDto cardUpdateDto)
    {
        var card = await bankContext.Cards.Include(c => c.User).Include(c => c.BankAccount)
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber) ?? throw new HttpException(404, "Card not found");

        if (cardUpdateDto.UserId != null)
        {
            _ = await bankContext.Users.FindAsync(cardUpdateDto.UserId) ??
                throw new HttpException(404, "User not found");
        }
        else
        {
            cardUpdateDto.UserId = card.UserId;
        }

        if (cardUpdateDto.BankAccountIban != null)
        {
            _ = await bankContext.BankAccounts.FindAsync(cardUpdateDto.BankAccountIban) ??
                throw new HttpException(404, "Bank account not found");
        }
        else
        {
            cardUpdateDto.BankAccountIban = card.BankAccountIban;
        }

        IsValid(cardUpdateDto);
        _mapper.Map(cardUpdateDto, card);
        await bankContext.SaveChangesAsync();
        return _mapper.Map<CardResponseDto>(card);
    }

    public async Task DeleteCard(string cardNumber)
    {
        var card = await bankContext.Cards.FirstAsync(c => c.CardNumber == cardNumber) ??
                   throw new HttpException(404, "Card not found");
        card.IsDeleted = true;
        await bankContext.SaveChangesAsync();
    }

    public async Task ActivateCard(string cardNumber)
    {
        var card = await bankContext.Cards.FirstAsync(c => c.CardNumber == cardNumber) ??
                   throw new HttpException(404, "Card not found");
        card.IsDeleted = false;
        await bankContext.SaveChangesAsync();
    }

    public async Task BlockCard(string cardNumber)
    {
        var card = await bankContext.Cards.FirstAsync(c => c.CardNumber == cardNumber) ??
                   throw new HttpException(404, "Card not found");
        if (card.IsBlocked)
        {
            throw new HttpException(400, "Card is already blocked");
        }

        card.IsBlocked = true;
        await bankContext.SaveChangesAsync();
    }

    public async Task UnblockCard(string cardNumber)
    {
        var card = await bankContext.Cards.FirstAsync(c => c.CardNumber == cardNumber) ??
                   throw new HttpException(404, "Card not found");
        if (!card.IsBlocked)
        {
            throw new HttpException(400, "Card is not blocked");
        }

        card.IsBlocked = false;
        await bankContext.SaveChangesAsync();
    }

    public async Task<CardResponseDto> RenovateCard(string cardNumber)
    {
        var card = await bankContext.Cards.FirstAsync(c => c.CardNumber == cardNumber) ??
                   throw new HttpException(404, "Card not found");
        if (card.ExpirationDate > DateTime.Now.AddMonths(3).ToUniversalTime())
        {
            throw new HttpException(400, "Card is not expired");
        }

        card.ExpirationDate = DateTime.Now.AddYears(4).ToUniversalTime();
        await bankContext.SaveChangesAsync();
        return _mapper.Map<CardResponseDto>(card);
    }

    private static void IsValid(CardCreateDto cardCreateDto)
    {
        if (!Enum.TryParse(typeof(CardType), cardCreateDto.CardType, out _))
        {
            throw new HttpException(400,
                "Invalid card type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(CardType))));
        }
    }

    private static void IsValid(CardUpdateDto cardUpdateDto)
    {
        if (!Enum.TryParse(typeof(CardType), cardUpdateDto.CardType, out _))
        {
            throw new HttpException(400,
                "Invalid card type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(CardType))));
        }
    }
}