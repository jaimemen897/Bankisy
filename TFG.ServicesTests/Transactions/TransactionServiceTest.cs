using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
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
    private BankAccountService _bankAccountService;
    private CardService _cardService;
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
        _cardService = new CardService(_mockContext.Object);
        _bankAccountService = new BankAccountService(_mockContext.Object, _cacheMock.Object, _cardService);
        _transactionService = new TransactionService(_mockContext.Object, _cacheMock.Object, _mockHubContext.Object, _bankAccountService);
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
        var result = await _transactionService.GetTransactions(1, 2, "Id", false, null, null, null);

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
    
    
    //GET TRANSACTIONS BY IBAN
    [Test]
    public async Task GetTransactionsByIban_ReturnsExpectedTransactions()
    {
        // Arrange
        var user = new User { Name = "Test User" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1, Amount = 100, Concept = "Test Transaction 1", IbanAccountOrigin = "ES123456789",
                Date = DateTime.Now, IbanAccountDestination = bankAccounts[0].Iban
            },
            new()
            {
                Id = 2, Amount = 200, Concept = "Test Transaction 2", IbanAccountDestination = "ES987654321",
                Date = DateTime.Now, IbanAccountOrigin = bankAccounts[1].Iban
            }
        };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);
        _mockContext.Setup(x => x.Transactions).ReturnsDbSet(transactions);

        // Act
        var result = await _transactionService.GetTransactionsByIban(bankAccounts[0].Iban, user.Id);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        });
    }
    
    [Test]
    public void GetTransactionsByIban_ThrowsHttpExceptionWhenAccountNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _transactionService.GetTransactionsByIban("ES1234567890123456789013", user.Id));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("BankAccount not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }
    
    [Test]
    public void GetTransactionsByIban_ThrowsHttpExceptionWhenUserIsNotOwner()
    {
        // Arrange
        var user = new User { Name = "Test User" };
        var user2 = new User { Name = "Test User 2" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _transactionService.GetTransactionsByIban("ES1234567890123456789012", Guid.NewGuid()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("You are not the owner of the account"));
            Assert.That(ex.Code, Is.EqualTo(403));
        });
    }
    
    
    //GET EXPENSES BY USERID
    [Test]
    public async Task GetExpensesByUserId_ReturnsExpectedTransactions()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1, Amount = 100, Concept = "Test Transaction 1", IbanAccountOrigin = bankAccounts[0].Iban,
                Date = DateTime.UtcNow, IbanAccountDestination = bankAccounts[0].Iban
            },
            new()
            {
                Id = 2, Amount = 200, Concept = "Test Transaction 2", IbanAccountDestination = bankAccounts[0].Iban,
                Date = DateTime.UtcNow, IbanAccountOrigin = bankAccounts[1].Iban
            }
        };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);
        _mockContext.Setup(x => x.Transactions).ReturnsDbSet(transactions);
        
        // Act
        var result = await _transactionService.GetExpensesByUserId(user.Id);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        });
    }
    
    
    //GET INCOMES BY USERID
    [Test]
    public async Task GetIncomesByUserId_ReturnsExpectedTransactions()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1, Amount = 100, Concept = "Test Transaction 1", IbanAccountOrigin = bankAccounts[1].Iban,
                Date = DateTime.UtcNow, IbanAccountDestination = bankAccounts[0].Iban
            },
            new()
            {
                Id = 2, Amount = 200, Concept = "Test Transaction 2", IbanAccountDestination = bankAccounts[1].Iban,
                Date = DateTime.UtcNow, IbanAccountOrigin = bankAccounts[0].Iban
            }
        };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);
        _mockContext.Setup(x => x.Transactions).ReturnsDbSet(transactions);
        
        // Act
        var result = await _transactionService.GetIncomesByUserId(user.Id);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        });
    }
    
    
    //CREATE TRANSACTION
    [Test]
    public async Task CreateTransaction_ReturnsExpectedTransaction()
    {
        // Arrange
        var transactionCreateDto = new TransactionCreateDto 
        { 
            IbanAccountOrigin = "ES1234567891234567891234", 
            IbanAccountDestination = "ES9876543219876543219876", 
            Amount = 100 
        };
        
        var accountOrigin = new BankAccount { Iban = "ES1234567891234567891234" };
        var accountDestination = new BankAccount { Iban = "ES9876543219876543219876" };
        
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
    
        var mockSetUsers = new Mock<DbSet<User>>();
        mockSetUsers.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);
        
        var mockSetAccounts = new Mock<DbSet<BankAccount>>();
        mockSetAccounts.Setup(x => x.FindAsync("ES1234567891234567891234")).ReturnsAsync(accountOrigin);
        mockSetAccounts.Setup(x => x.FindAsync("ES9876543219876543219876")).ReturnsAsync(accountDestination);
        
        _mockContext.Setup(x => x.Users).Returns(mockSetUsers.Object);
        _mockContext.Setup(x => x.BankAccounts).Returns(mockSetAccounts.Object);
        
        // Act
        var result = await _transactionService.CreateTransaction(transactionCreateDto, userId);
    
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Amount, Is.EqualTo(100));
            Assert.That(result.IbanAccountOrigin, Is.EqualTo("ES1234567891234567891234"));
            Assert.That(result.IbanAccountDestination, Is.EqualTo("ES9876543219876543219876"));
        });
    }
    
    [Test]
    public void CreateTransaction_ThrowsHttpExceptionWhenAccountOriginNotFound()
    {
        // Arrange
        var transactionCreateDto = new TransactionCreateDto 
        { 
            IbanAccountOrigin = "ES1234567891234567891234", 
            IbanAccountDestination = "ES9876543219876543219876", 
            Amount = 100 
        };

        var accountDestination = new BankAccount { Iban = "ES9876543219876543219876" };
        
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { accountDestination });
        
        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _transactionService.CreateTransaction(transactionCreateDto, Guid.NewGuid()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Account origin not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }
    
    
    //DELETE TRANSACTION
    [Test]
    public async Task DeleteTransaction_ReturnsExpectedTransaction()
    {
        // Arrange
        var transaction = new Transaction { Id = 1, Amount = 100, Concept = "Test Transaction 1" };
        var mockSet = new Mock<DbSet<Transaction>>();
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync(transaction);
        _mockContext.Setup(x => x.Transactions).Returns(mockSet.Object);

        // Act
        await _transactionService.DeleteTransaction(1);

        // Assert
        mockSet.Verify(x => x.Remove(transaction), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        
    }
    
    [Test]
    public void DeleteTransaction_ThrowsHttpExceptionWhenTransactionNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Transaction>>();
        mockSet.Setup(x => x.FindAsync(1)).ReturnsAsync((Transaction)null);
        _mockContext.Setup(x => x.Transactions).Returns(mockSet.Object);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _transactionService.DeleteTransaction(1));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Transaction not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

}