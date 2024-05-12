using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;

namespace TFG.ServicesTests.Users;

[TestFixture]
public class UserServiceTest
{
    private Mock<IMemoryCache> _cacheMock;
    private UsersService _usersService;
    private BankAccountService _bankAccountService;
    private CardService _cardService;
    private Mock<BankContext> _mockContext;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _mockContext = new Mock<BankContext>(options);
        _cardService = new CardService(_mockContext.Object);
        _bankAccountService = new BankAccountService(_mockContext.Object, _cacheMock.Object, _cardService);
        _usersService = new UsersService(_mockContext.Object, _cacheMock.Object, _bankAccountService);
    }

    //GET ALL USERS
    [Test]
    public async Task GetUsers_ReturnsExpectedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var result = await _usersService.GetUsers(1, 2, "Name", false, "Test User");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items[0].Name, Is.EqualTo("Test User 1"));
            Assert.That(result.Items[1].Name, Is.EqualTo("Test User 2"));
        });
    }

    [Test]
    public async Task GetUsers_ReturnsUsersFilteredByName()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var result = await _usersService.GetUsers(1, 2, "Name", false, "Test User 1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Name, Is.EqualTo("Test User 1"));
        });
    }

    [Test]
    public async Task GetUsers_ReturnsUsersFilteredByEmail()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var result = await _usersService.GetUsers(1, 2, "Name", false, "test1@test.com");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Email, Is.EqualTo("test1@test.com"));
        });
    }

    [Test]
    public async Task GetUsers_ReturnsUsersFilteredByPhone()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var result = await _usersService.GetUsers(1, 2, "Phone", false, "123456789");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Phone, Is.EqualTo("123456789"));
        });
    }

    [Test]
    public void GetUsers_InvalidOrderByParameter()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var exception =
            Assert.ThrowsAsync<HttpException>(() => _usersService.GetUsers(1, 2, "Invalid", false, "Test User"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Invalid orderBy parameter"));
        });
    }

    [Test]
    public void GetUsers_EmptyList()
    {
        // Arrange
        _mockContext.Setup(x => x.Users).ReturnsDbSet([]);

        // Act
        var result = _usersService.GetUsers(1, 2, "Name", false, "Test User");

        // Assert
        Assert.That(result.Result.Items, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task GetAllUsers_ReturnsExpectedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test1@test.com", Phone = "123456789" },
            new() { Id = Guid.NewGuid(), Name = "Test User 2", Email = "test2@test.com", Phone = "987654321" }
        };
        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var result = await _usersService.GetAllUsers();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Test User 1"));
            Assert.That(result[1].Name, Is.EqualTo("Test User 2"));
        });
    }

    //GET USER BY ID
    [Test]
    public async Task GetUserAsync_ReturnsExpectedUser()
    {
        // Arrange
        var expectedUser = new User { Id = Guid.NewGuid(), Name = "Test User" };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(expectedUser.Id)).ReturnsAsync(expectedUser);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        // Act
        var result = await _usersService.GetUserAsync(expectedUser.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(expectedUser.Id));
            Assert.That(result.Name, Is.EqualTo(expectedUser.Name));
        });
    }

    [Test]
    public void GetUserAsync_ReturnsNull()
    {
        // Arrange
        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(It.IsAny<Guid>())).ReturnsAsync((User)null);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.GetUserAsync(Guid.NewGuid()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }

    //CREATE USER
    [Test]
    public async Task CreateUser_ReturnsExpectedUser()
    {
        // Arrange
        var user = new UserCreateDto
        {
            Name = "Test User",
            Email = "test@test.com",
            Username = "test",
            Dni = "54522318J",
            Gender = "Male",
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet([]);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.CreateUser(user);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(user.Name));
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Username, Is.EqualTo(user.Username));
            Assert.That(result.Dni, Is.EqualTo(user.Dni));
            Assert.That(result.Gender, Is.EqualTo(user.Gender));
            Assert.That(result.Avatar, Is.EqualTo(user.Avatar));
            Assert.That(result.Phone, Is.EqualTo(user.Phone));
        });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateUser_UsernameEmailOrDniAlreadyExists()
    {
        // Arrange
        var user = new UserCreateDto
        {
            Name = "Test User",
            Email = "test@test.com",
            Username = "test",
            Dni = "54522318J",
            Gender = "Male",
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
        };

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(), Name = "Test User", Email = "test@test.com", Username = "test", Dni = "54522318J",
                Gender = Gender.Male, Password = "password", Avatar = "avatar.png", Phone = "123456789"
            }
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(users);

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.CreateUser(user));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Username, Email or DNI already exists"));
        });
    }

    //UPDATE USER
    /*[Test]
    public async Task UpdateUser_ReturnsExpectedUser()
    {
        // Arrange
        var oldUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            Username = "test",
            Dni = "54522318J",
            Gender = Gender.Male,
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
        };

        var user = new UserUpdateDto()
        {
            Name = "Name updated",
            Email = "test@test.com2",
            Username = "username updated",
            Dni = "54522317J",
            Gender = "Male",
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
        };

        var expectedUser = new UserResponseDto()
        {
            Name = "Name updated",
            Email = "test@test.com2",
            Username = "username updated",
            Dni = "54522317J",
            Gender = "Male",
            Avatar = "avatar.png",
            Phone = "123456789",
        };

        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Name = "Test User", Email = "test@test.com", Username = "test", Dni = "54522318J", Gender = Gender.Male, Password = "password", Avatar = "avatar.png", Phone = "123456789" }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(oldUser.Id)).ReturnsAsync(users.First());
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        // Act
        var result = await _usersService.UpdateUser(oldUser.Id, user);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(expectedUser.Name));
            Assert.That(result.Email, Is.EqualTo(expectedUser.Email));
            Assert.That(result.Username, Is.EqualTo(expectedUser.Username));
            Assert.That(result.Dni, Is.EqualTo(expectedUser.Dni));
            Assert.That(result.Gender, Is.EqualTo(expectedUser.Gender));
            Assert.That(result.Avatar, Is.EqualTo(expectedUser.Avatar));
            Assert.That(result.Phone, Is.EqualTo(expectedUser.Phone));
        });
    }*/


    /*var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            Username = "test",
            Dni = "54522318J",
            Gender = Gender.Male,
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
            Role = Roles.User,
            IsDeleted = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };*/
}