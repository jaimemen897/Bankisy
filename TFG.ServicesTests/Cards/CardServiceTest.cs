using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.Models;
using TFG.Services;

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

    [Test]
    public async Task GetCard_ReturnsExpectedCard()
    {
        // Arrange
        var cardNumber = "1234567890123456";
        var card = new Card { CardNumber = cardNumber, Pin = "1SZmfpo8JtGvn0jxFDYomg==", Cvv = "qRuuPv6gwJ72CEgpD/QG9Q=="};
        var user = new User();
        var bankAccount = new BankAccount();
        card.User = user;
        card.BankAccount = bankAccount;

        var data = new List<Card> { card };

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(data);

        // Act
        var result = await _cardService.GetCardByCardNumber(cardNumber);

        // Assert
        Assert.AreEqual(cardNumber, result.CardNumber);
    }
}