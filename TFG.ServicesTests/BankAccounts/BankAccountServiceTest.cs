using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;

namespace TFG.ServicesTests.BankAccounts;

[TestFixture]
public class BankAccountServiceTest
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
            new()
            {
                Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current,
                Users = new List<User> { user }
            },
            new()
            {
                Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student,
                Users = new List<User> { user2 }
            }
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
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.GetBankAccounts(1, 2, "Invalid", false, "ES1234567890123456789012"));

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
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.GetBankAccount("ES1234567890123456789013"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("BankAccount not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    //GET BANK ACCOUNTS BY USER ID
    [Test]
    public async Task GetBankAccountsByUserId_ReturnsExpectedBankAccounts()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccounts = new List<BankAccount>
        {
            new()
            {
                Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new()
            {
                Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student, Users =
                    [user2]
            }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccountsByUserId(user.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Iban, Is.EqualTo("ES1234567890123456789012"));
        });
    }

    [Test]
    public async Task GetBankAccountsByUserId_ReturnsEmptyList_WhenUserHasNoBankAccounts()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccounts = new List<BankAccount>
        {
            new()
            {
                Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user2]
            },
            new()
            {
                Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student, Users =
                    [user2]
            }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetBankAccountsByUserId(user.Id);

        // Assert
        Assert.That(result, Is.Empty);
    }

    //GET TOTAL BALANCE BY USER ID
    [Test]
    public async Task GetTotalBalanceByUserId_ReturnsExpectedTotalBalance()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccounts = new List<BankAccount>
        {
            new()
            {
                Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user]
            },
            new()
            {
                Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student, Users =
                    [user2]
            }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetTotalBalanceByUserId(user.Id);

        // Assert
        Assert.That(result, Is.EqualTo(1000));
    }

    [Test]
    public async Task GetTotalBalanceByUserId_ReturnsZero_WhenUserHasNoBankAccounts()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccounts = new List<BankAccount>
        {
            new()
            {
                Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, Users = [user2]
            },
            new()
            {
                Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Student, Users =
                    [user2]
            }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var result = await _bankAccountService.GetTotalBalanceByUserId(user.Id);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    //ACTIVATE BANK ACCOUNT
    [Test]
    public async Task ActivateBankAccount_Ok()
    {
        // Arrange
        var bankAccount = new BankAccount
            { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current, IsDeleted = true };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        await _bankAccountService.ActivateBankAccount(bankAccount.Iban);

        // Assert
        Assert.That(bankAccount.IsDeleted, Is.False);
    }

    [Test]
    public void ActivateBankAccount_ThrowsException_WhenBankAccountNotFound()
    {
        // Arrange
        var bankAccounts = new List<BankAccount>
        {
            new() { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current },
            new() { Iban = "ES9876543210987654321098", Balance = 2000, AccountType = AccountType.Saving }
        };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(bankAccounts);

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.ActivateBankAccount("ES1234567890123456789013"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Bank account not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    //DELETE BANK ACCOUNT
    [Test]
    public async Task DeleteBankAccount_Ok()
    {
        // Arrange
        var bankAccount = new BankAccount
            { Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current };
        var cards = new List<Card> { new() { CardNumber = "1234567890123456", BankAccount = bankAccount } };
        bankAccount.Cards = cards;
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(cards);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        await _bankAccountService.DeleteBankAccount(bankAccount.Iban);

        // Assert
        Assert.That(bankAccount.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteBankAccount_Ok_WhenBankAccountHasNoCards()
    {
        // Arrange
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current,
            Cards = []
        };
        _mockContext.Setup(x => x.Cards).ReturnsDbSet([]);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        await _bankAccountService.DeleteBankAccount(bankAccount.Iban);

        // Assert
        Assert.That(bankAccount.IsDeleted, Is.False);
    }

    [Test]
    public void DeleteBankAccount_ThrowsException_WhenBankAccountHasBalance()
    {
        // Arrange
        var bankAccount = new BankAccount
            { Iban = "ES1234567890123456789012", Balance = 1000, AccountType = AccountType.Current };
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.DeleteBankAccount("ES1234567890123456789012"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("You can't delete a bank account with balance"));
            Assert.That(ex.Code, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task DeleteBankAccount_Ok_WhenUserIdIsNotNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current,
            Cards = [], Users = [user]
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.Cards).ReturnsDbSet(new List<Card>());
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        await _bankAccountService.DeleteBankAccount(bankAccount.Iban, userId);

        // Assert
        Assert.That(bankAccount.IsDeleted, Is.False);
    }

    //ACTIVATE BIZUM
    [Test]
    public async Task ActivateBizum_Ok()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        await _bankAccountService.ActiveBizum(bankAccount.Iban, user.Id);

        // Assert
        Assert.That(bankAccount.AcceptBizum, Is.True);
    }

    [Test]
    public void ActivateBizum_ThrowsException_WhenBankAccountNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.ActiveBizum("ES1234567890123456789013", user.Id));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Bank account not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void ActivateBizum_ThrowsException_WhenUserNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.ActiveBizum(bankAccount.Iban, Guid.NewGuid()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("User not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void ActivateBizum_ThrowsException_WhenUserNotFoundInBankAccount()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var users = new List<User> { user2 };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet([user]);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.ActiveBizum(bankAccount.Iban, user.Id));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("User not found in bank account"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    //CREATE
    [Test]
    public async Task CreateBankAccount_Ok()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccountCreateDto { AccountType = "Current", UsersId = [user.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var result = await _bankAccountService.CreateBankAccount(bankAccount);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Iban, Is.Not.Null);
            Assert.That(result.Balance, Is.EqualTo(0));
            Assert.That(result.AccountType, Is.EqualTo(AccountType.Current.ToString()));
            Assert.That(result.AcceptBizum, Is.True);
            Assert.That(result.UsersId, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void CreateBankAccount_ThrowsException_WhenUsersNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var bankAccount = new BankAccountCreateDto { AccountType = "Current", UsersId = [user.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.CreateBankAccount(bankAccount));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Users not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void CreateBankAccount_ThrowsException_WhenUserNotFoundInUsers()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccount = new BankAccountCreateDto { AccountType = "Current", UsersId = [user.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user2 });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.CreateBankAccount(bankAccount));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Users not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void CreateBankAccount_ThrowsException_WhenUserNotFoundInBankAccount()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccount = new BankAccountCreateDto
            { AccountType = "Current", UsersId = [user2.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.CreateBankAccount(bankAccount));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Users not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void CreateBankAccount_ThrowsException_WhenInvalidAccountType()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccountCreateDto { AccountType = "Invalid", UsersId = [user.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.CreateBankAccount(bankAccount));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message,
                Is.EqualTo("Invalid account type. Valid values are: " +
                           string.Join(", ", Enum.GetNames(typeof(AccountType)))));
            Assert.That(ex.Code, Is.EqualTo(400));
        });
    }

    [Test]
    public void CreateBankAccount_ThrowsException_WhenUserTriesToCreateBankAccountForAnotherUser()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var user2 = new User { Name = "Test User 2", Id = Guid.NewGuid() };
        var bankAccount = new BankAccountCreateDto
            { AccountType = "Current", UsersId = [user2.Id], AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user, user2 });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() => _bankAccountService.CreateBankAccount(bankAccount, user.Id));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("You can't create a bank account for another user"));
            Assert.That(ex.Code, Is.EqualTo(403));
        });
    }

    //UPDATE
    [Test]
    public async Task UpdateBankAccount_Ok()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };
        var bankAccountUpdate = new BankAccountUpdateDto
            { AccountType = "Saving", UsersId = new List<Guid> { user.Id }, AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var result = await _bankAccountService.UpdateBankAccount(bankAccount.Iban, bankAccountUpdate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Iban, Is.EqualTo(bankAccount.Iban));
            Assert.That(result.Balance, Is.EqualTo(bankAccount.Balance));
            Assert.That(result.AccountType, Is.EqualTo(AccountType.Saving.ToString()));
            Assert.That(result.AcceptBizum, Is.True);
            Assert.That(result.UsersId, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void UpdateBankAccount_ThrowsException_WhenBankAccountNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccountUpdate = new BankAccountUpdateDto
            { AccountType = "Saving", UsersId = new List<Guid> { user.Id }, AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.UpdateBankAccount("ES1234567890123456789013", bankAccountUpdate));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Bank account not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void UpdateBankAccount_ThrowsException_WhenUsersNotFound()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = new List<User> { user }
        };
        var bankAccountUpdate = new BankAccountUpdateDto
            { AccountType = "Saving", UsersId = new List<Guid> { Guid.NewGuid() }, AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.UpdateBankAccount(bankAccount.Iban, bankAccountUpdate));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Users not found"));
            Assert.That(ex.Code, Is.EqualTo(404));
        });
    }

    [Test]
    public void UpdateBankAccount_ThrowsException_WhenInvalidAccountType()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var users = new List<User> { user };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current, AcceptBizum = false,
            Users = users
        };
        var bankAccountUpdate = new BankAccountUpdateDto
            { AccountType = "Invalid", UsersId = new List<Guid> { user.Id }, AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var ex = Assert.ThrowsAsync<HttpException>(() =>
            _bankAccountService.UpdateBankAccount(bankAccount.Iban, bankAccountUpdate));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message,
                Is.EqualTo("Invalid account type. Valid values are: " +
                           string.Join(", ", Enum.GetNames(typeof(AccountType)))));
            Assert.That(ex.Code, Is.EqualTo(400));
        });
    }
    
    [Test]
    public async Task UpdateBankAccount_Ok_WhenUsersIdIsNull()
    {
        // Arrange
        var user = new User { Name = "Test User", Id = Guid.NewGuid() };
        var bankAccount = new BankAccount
        {
            Iban = "ES1234567890123456789012", Balance = 0, AccountType = AccountType.Current
        };
        var bankAccountUpdate = new BankAccountUpdateDto { AccountType = "Saving", UsersId = null, AcceptBizum = true };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount> { bankAccount });

        // Act
        var result = await _bankAccountService.UpdateBankAccount(bankAccount.Iban, bankAccountUpdate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Iban, Is.EqualTo(bankAccount.Iban));
            Assert.That(result.Balance, Is.EqualTo(bankAccount.Balance));
            Assert.That(result.AccountType, Is.EqualTo(AccountType.Saving.ToString()));
            Assert.That(result.AcceptBizum, Is.True);
            Assert.That(result.UsersId, Is.Empty);
        });
    }
}