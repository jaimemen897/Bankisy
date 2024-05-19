using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.cards;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;

namespace TFG.ServicesTests.Cards;

[TestFixture]
public class CardServiceTest
{
    private CardService _cardService;
    private Mock<BankContext> _mockContext;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _mockContext = new Mock<BankContext>(options);
        _cardService = new CardService(_mockContext.Object);
    }

    //GET ALL
    [Test]
    public async Task GetCards_ReturnsExpectedCards()
    {
        // Arrange
        var cards = new List<Card>
        {
            new()
            {
                CardNumber = "1234567890123456", User = new User { Name = "Test User 1" },
                BankAccount = new BankAccount { Iban = "ES1234567891234567891234" }
            },
            new()
            {
                CardNumber = "2345678901234567", User = new User { Name = "Test User 2" },
                BankAccount = new BankAccount { Iban = "ES1234567891234567891235" }
            }
        };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(cards);

        // Act
        var result = await _cardService.GetCards(1, 2, "CardNumber", false, "Test");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items[0].CardNumber, Is.EqualTo("1234567890123456"));
            Assert.That(result.Items[1].CardNumber, Is.EqualTo("2345678901234567"));
        });
    }

    [Test]
    public void GetCards_InvalidOrderByParameter()
    {
        // Arrange
        var cards = new List<Card>
        {
            new() { CardNumber = "1234567890123456", UserId = Guid.NewGuid() },
            new() { CardNumber = "2345678901234567", UserId = Guid.NewGuid() }
        };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(cards);

        // Act
        var exception =
            Assert.ThrowsAsync<HttpException>(() => _cardService.GetCards(1, 2, "Invalid", false, "1234567890123456"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Invalid orderBy parameter"));
        });
    }

    [Test]
    public void GetCards_EmptyList()
    {
        // Arrange
        _mockContext.Setup(x => x.Cards).ReturnsDbSet([]);

        // Act
        var result = _cardService.GetCards(1, 2, "CardNumber", false, "1234567890123456");

        // Assert
        Assert.That(result.Result.Items, Has.Count.EqualTo(0));
    }
    

    //GET BY CARD NUMBER
    [Test]
    public async Task GetCard_ReturnsExpectedCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card
            { CardNumber = cardNumber, Pin = "1SZmfpo8JtGvn0jxFDYomg==", Cvv = "qRuuPv6gwJ72CEgpD/QG9Q==" };
        var user = new User();
        var bankAccount = new BankAccount();
        card.User = user;
        card.BankAccount = bankAccount;

        var data = new List<Card> { card };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(data);

        // Act
        var result = await _cardService.GetCardByCardNumber(cardNumber);

        // Assert
        Assert.That(result.CardNumber, Is.EqualTo(cardNumber));
    }

    [Test]
    public void GetCardByCardNumber_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet([]);

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.GetCardByCardNumber(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }


    //GET BY USER ID
    [Test]
    public async Task GetCardsByUserId_ReturnsExpectedCards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = new List<Card>
        {
            new() { CardNumber = "1234567890123456", UserId = userId },
            new() { CardNumber = "2345678901234567", UserId = userId }
        };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(cards);

        // Act
        var result = await _cardService.GetCardsByUserId(userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].CardNumber, Is.EqualTo("1234567890123456"));
            Assert.That(result[1].CardNumber, Is.EqualTo("2345678901234567"));
        });
    }

    [Test]
    public async Task GetCardsByUserId_CardsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockContext.Setup(x => x.Cards).ReturnsDbSet([]);

        // Act
        var result = await _cardService.GetCardsByUserId(userId);

        // Assert
        Assert.Multiple(() => { Assert.That(result, Has.Count.EqualTo(0)); });
    }


    //GET BY IBAN
    [Test]
    public async Task GetCardsByIban_ReturnsExpectedCards()
    {
        // Arrange
        var iban = "ES1234567891234567891234";
        var cards = new List<Card>
        {
            new() { CardNumber = "1234567890123456", BankAccountIban = iban },
            new() { CardNumber = "2345678901234567", BankAccountIban = iban }
        };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(cards);

        // Act
        var result = await _cardService.GetCardsByIban(iban);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].CardNumber, Is.EqualTo("1234567890123456"));
            Assert.That(result[1].CardNumber, Is.EqualTo("2345678901234567"));
        });
    }

    [Test]
    public async Task GetCardsByIban_CardsNotFound()
    {
        // Arrange
        var iban = "ES1234567891234567891234";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet([]);

        // Act
        var result = await _cardService.GetCardsByIban(iban);

        // Assert
        Assert.Multiple(() => { Assert.That(result, Has.Count.EqualTo(0)); });
    }


    //CREATE
    [Test]
    public async Task CreateCard_ReturnsExpectedCard()
    {
        var userId = Guid.NewGuid();

        // Arrange
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234",
            CardType = "Debit",
            Pin = "1SZmfpo8JtGvn0jxFDYomg=="
        };

        var user = new User { Id = userId };
        var bankAccount = new BankAccount { Iban = cardCreateDto.BankAccountIban, Users = [user] };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var result = await _cardService.CreateCard(cardCreateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.CardNumber, Is.Not.Null);
            Assert.That(result.Cvv, Is.Not.Null);
            Assert.That(result.Pin, Is.Not.Null);
            Assert.That(result.CardType, Is.EqualTo(cardCreateDto.CardType));
        });
    }

    [Test]
    public void CreateCard_ThrowsHttpException_UserNotFound()
    {
        var userId = Guid.NewGuid();

        // Arrange
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234"
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.CreateCard(cardCreateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }

    [Test]
    public void CreateCard_ThrowsHttpException_UserDontBelongToBankaccount()
    {
        var userId = Guid.NewGuid();

        // Arrange
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234"
        };

        var user = new User { Id = userId };
        var bankAccount = new BankAccount { Iban = cardCreateDto.BankAccountIban };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.CreateCard(cardCreateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Bank account does not belong to the user"));
        });
    }

    [Test]
    public void CreateCard_ThrowsHttpException_BankAccountAlreadyHasCard()
    {
        var userId = Guid.NewGuid();

        // Arrange
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234"
        };

        var user = new User { Id = userId };
        var bankAccount = new BankAccount { Iban = cardCreateDto.BankAccountIban, Users = [user] };
        var card = new Card { BankAccountIban = cardCreateDto.BankAccountIban };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.CreateCard(cardCreateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Bank account already has a card"));
        });
    }

    [Test]
    public void CreateCard_ThrowsHttpException_BankAccountNotFound()
    {
        var userId = Guid.NewGuid();

        // Arrange
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234"
        };

        var user = new User { Id = userId };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.CreateCard(cardCreateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Bank account not found"));
        });
    }

    [Test]
    public void CreateCard_ThrowsHttpException_InvalidCardType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cardCreateDto = new CardCreateDto
        {
            UserId = userId,
            BankAccountIban = "ES1234567891234567891234",
            CardType = "InvalidCardType"
        };

        var user = new User { Id = userId };
        var bankAccount = new BankAccount { Iban = cardCreateDto.BankAccountIban, Users = [user] };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(cardCreateDto.UserId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.CreateCard(cardCreateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message,
                Is.EqualTo("Invalid card type. Valid values are: " +
                           string.Join(", ", Enum.GetNames(typeof(CardType)))));
        });
    }


    //UPDATE
    [Test]
    public async Task UpdateCard_ReturnsExpectedCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { UserId = Guid.NewGuid(), CardType = "Visa" };
        var card = new Card { CardNumber = cardNumber, UserId = Guid.NewGuid(), CardType = CardType.Debit };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(cardUpdateDto.UserId)).ReturnsAsync(new User());

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var result = await _cardService.UpdateCard(cardNumber, cardUpdateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.CardNumber, Is.EqualTo(cardNumber));
            Assert.That(result.CardType, Is.EqualTo(cardUpdateDto.CardType));
        });
    }

    [Test]
    public void UpdateCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { UserId = Guid.NewGuid(), CardType = "Visa" };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UpdateCard(cardNumber, cardUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }

    [Test]
    public void UpdateCard_ThrowsHttpException_UserNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { UserId = Guid.NewGuid(), CardType = "Visa" };
        var card = new Card { CardNumber = cardNumber, UserId = Guid.NewGuid(), CardType = CardType.Debit };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });
        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UpdateCard(cardNumber, cardUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }

    [Test]
    public void UpdateCard_ThrowsHttpException_BankAccountNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { UserId = Guid.NewGuid(), BankAccountIban = "ES1234567891234567891234" };
        var card = new Card { CardNumber = cardNumber, UserId = Guid.NewGuid(), CardType = CardType.Debit };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(cardUpdateDto.UserId)).ReturnsAsync(new User());

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UpdateCard(cardNumber, cardUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Bank account not found"));
        });
    }

    [Test]
    public void UpdateCard_ThrowsHttpException_InvalidCardType()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { UserId = Guid.NewGuid(), CardType = "InvalidCardType" };
        var card = new Card { CardNumber = cardNumber, UserId = Guid.NewGuid(), CardType = CardType.Debit };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(cardUpdateDto.UserId)).ReturnsAsync(new User());

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UpdateCard(cardNumber, cardUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message,
                Is.EqualTo("Invalid card type. Valid values are: " +
                           string.Join(", ", Enum.GetNames(typeof(CardType)))));
        });
    }

    [Test]
    public async Task UpdateCard_KeepsSameUserId_WhenUserIdNotProvidedInCardUpdateDto()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var cardUpdateDto = new CardUpdateDto { CardType = "Visa", Pin = "1SZmfpo8JtGvn0jxFDYomg==" };
        var existingUserId = Guid.NewGuid();
        var card = new Card { CardNumber = cardNumber, UserId = existingUserId, CardType = CardType.Debit };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var result = await _cardService.UpdateCard(cardNumber, cardUpdateDto);

        // Assert
        Assert.That(result.Pin, Is.EqualTo(cardUpdateDto.Pin));
    }
    
    
    //DELETE
    [Test]
    public async Task DeleteCard_DeletesCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        await _cardService.DeleteCard(cardNumber);

        // Assert
        Assert.That(card.IsDeleted, Is.True);
    }

    [Test]
    public void DeleteCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.DeleteCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }
    
    
    //ACTIVATE
    [Test]
    public async Task ActivateCard_ActivatesCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, IsDeleted = true };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        await _cardService.ActivateCard(cardNumber);

        // Assert
        Assert.That(card.IsDeleted, Is.False);
    }
    
    [Test]
    public void ActivateCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.ActivateCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }
    
    
    //BLOCK
    [Test]
    public async Task BlockCard_BlocksCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, IsBlocked = false };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        await _cardService.BlockCard(cardNumber);

        // Assert
        Assert.That(card.IsBlocked, Is.True);
    }

    [Test]
    public void BlockCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.BlockCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }
    
    [Test]
    public void BlockCard_ThrowsHttpException_CardAlreadyBlocked()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, IsBlocked = true };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.BlockCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Card is already blocked"));
        });
    }
    
    
    //UNBLOCK
    [Test]
    public async Task UnblockCard_UnblocksCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, IsBlocked = true };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        await _cardService.UnblockCard(cardNumber);

        // Assert
        Assert.That(card.IsBlocked, Is.False);
    }

    [Test]
    public void UnblockCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UnblockCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }
    
    [Test]
    public void UnblockCard_ThrowsHttpException_CardNotBlocked()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, IsBlocked = false };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.UnblockCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Card is not blocked"));
        });
    }
    
    
    //RENOVATE
    [Test]
    public async Task RenovateCard_ReturnsExpectedCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var result = await _cardService.RenovateCard(cardNumber);

        // Assert
        Assert.That(result.CardNumber, Is.EqualTo(cardNumber));
    }
    
    [Test]
    public void RenovateCard_ThrowsHttpException_CardNotFound()
    {
        // Arrange
        var cardNumber = "1234567890123456";

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.RenovateCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("Card not found"));
        });
    }
    
    [Test]
    public void RenovateCard_ThrowsHttpException_CardNotExpired()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, ExpirationDate = DateTime.Now.AddMonths(5).ToUniversalTime() };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card> { card });

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _cardService.RenovateCard(cardNumber));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Card is not expired"));
        });
    }
}