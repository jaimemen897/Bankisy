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
        var card = new Card { CardNumber = "1234567890123456", Pin = "1234" };
        
        var mockSet = new Mock<DbSet<Card>>();
        mockSet.Setup(x => x.FirstAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Card, bool>>>(), default))
            .ReturnsAsync(card);

        _mockContext.Setup(x => x.Cards).ReturnsDbSet(mockSet.Object);

        // Act
        var result = await _cardService.GetCardByCardNumber(card.CardNumber);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.CardNumber, Is.EqualTo(card.CardNumber));
            Assert.That(result.Pin, Is.EqualTo(card.Pin));
        });
    }
}