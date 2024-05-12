using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;

namespace TFG.ServicesTests.BankAccounts;

[TestFixture]
public class UserServiceTest
{
    private BankAccountService _bankAccountService;
    private Mock<BankContext> _mockContext;
    private Mock<IMemoryCache> _cacheMock;
    private Mock<CardService> _mockCardService;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _mockContext = new Mock<BankContext>(options);
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _mockCardService = new Mock<CardService>(_mockContext.Object);
        _bankAccountService = new BankAccountService(_mockContext.Object, _cacheMock.Object, _mockCardService.Object);
    }
    
    //GET ALL BANK ACCOUNTS
    [Test]
    public async Task GetBankAccounts_ReturnsExpectedBankAccounts()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccounts(1, 2, "Iban", false, "ES1234567890123456789012");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Iban, Is.EqualTo("ES1234567890123456789012"));
        });
    }
    
    [Test]
    public async Task GetBankAccounts_ReturnsBankAccountsFilteredByBalance()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Student },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccounts(1, 2, "Balance", false, "1000");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Balance, Is.EqualTo(1000));
        });
    }

    [Test]
    public async Task GetBankAccounts_ReturnsBankAccountsFilteredByIban()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccounts(1, 2, "Iban", false, "ES1234567890123456789012");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Iban, Is.EqualTo("ES1234567890123456789012"));
        });
    }

    [Test]
    public async Task GetBankAccounts_ReturnsBankAccountsFilteredByUser()
    {
        // Arrange
        var user = new User { Name = "Test User" };
        var user2 = new User { Name = "Test User 2" };
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = new List<User> { user } },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student , Users = new List<User> { user2 } }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccounts(1, 2, "Iban", false, "Test User");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Iban, Is.EqualTo("ES1234567890123456789012"));
        });
    }
    
    [Test]
    public void GetBankAccounts_ThrowsException_WhenInvalidOrderByParameter()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.GetBankAccounts(1, 2, "Invalid", false, "ES1234567890123456789012"));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Invalid orderBy parameter"));
    }
    
    //GET BANK ACCOUNT BY IBAN
    [Test]
    public async Task GetBankAccountByIban_ReturnsExpectedBankAccount()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccount("ES1234567890123456789012");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Iban, Is.EqualTo("ES1234567890123456789012"));
            Assert.That(result.Balance, Is.EqualTo(1000));
        });
    }
    
    [Test]
    public void GetBankAccountByIban_ThrowsException_WhenBankAccountNotFound()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);
        
        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.GetBankAccount("ES1234567890123456789013"));
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("BankAccount not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });

    }
}