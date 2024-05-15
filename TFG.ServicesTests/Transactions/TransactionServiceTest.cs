using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;
using TFG.Services.Hub;

namespace TFG.ServicesTests.Transactions;

[TestFixture]
public class TransactionServiceTest
{
    private Mock<BankContext> _mockContext;
    private Mock<IMemoryCache> _cacheMock;
    private TransactionService _transactionService;
    private Mock<IHubContext<MyHub>> _mockHubContext;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _mockContext = new Mock<BankContext>(options);
        _mockHubContext = new Mock<IHubContext<MyHub>>();
        _transactionService = new TransactionService(_mockContext.Object, _cacheMock.Object, _mockHubContext.Object);
    }

    //GET ALL TRANSACTIONS
    [Test]
    public async Task GetTransactions_ReturnsExpectedTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1, Amount = 100, Concept = "Test Transaction 1", IbanAccountOrigin = "ES123456789",
                Date = DateTime.Now, IbanAccountDestination = "ES987654321"
            },
            new()
            {
                Id = 2, Amount = 200, Concept = "Test Transaction 2", IbanAccountDestination = "ES987654321",
                Date = DateTime.Now, IbanAccountOrigin = "ES123456789"
            }
        };

        _mockContext.Setup(x => x.Transactions).ReturnsDbSet(transactions);

        // Act
        var result = await _transactionService.GetTransactions(1, 2, "Id", false, "Test");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items[0].Id, Is.EqualTo(1));
            Assert.That(result.Items[1].Id, Is.EqualTo(2));
        });
    }

    //GET TRANSACTION BY ID
    [Test]
    public async Task GetTransaction_ReturnsExpectedTransaction()
    {
        // Arrange
        var transaction = new Transaction { Id = 1, Amount = 100, Concept = "Test Transaction 1" };
        var mockSet = new Mock<DbSet<Transaction>>();
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync(transaction);
        _mockContext.Setup(x => x.Transactions).Returns(mockSet.Object);

        // Act
        var result = await _transactionService.GetTransaction(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Amount, Is.EqualTo(100));
            Assert.That(result.Concept, Is.EqualTo("Test Transaction 1"));
        });
    }

    //get transaction by id not found
    [Test]
    public void GetTransaction_ThrowsHttpExceptionWhenTransactionNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Transaction>>();
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync((Transaction)null);
        _mockContext.Setup(x => x.Transactions).Returns(mockSet.Object);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _transactionService.GetTransaction(1));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Transaction not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }
}