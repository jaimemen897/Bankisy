using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
    private Mock<BankContext> _mockContext;
    private UsersService _usersService;
    private BankAccountService _bankAccountService;
    private CardService _cardService;
    private IConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _mockContext = new Mock<BankContext>(options);
        _cardService = new CardService(_mockContext.Object);

        Mock<IConfiguration> configurationMock = new Mock<IConfiguration>();
        //key, issuer, audience
        configurationMock.Setup(x => x["Jwt:Key"]).Returns("a_very_long_and_secure_key");
        configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
        configurationMock.Setup(x => x["Jwt:Audience"]).Returns("audience");
        configurationMock.Setup(x => x["Jwt:secret"]).Returns("a_very_long_and_secure_key_laskdjñflaksjdf");

        
        _configuration = configurationMock.Object;

        _bankAccountService = new BankAccountService(_mockContext.Object, _cacheMock.Object, _cardService);
        _usersService = new UsersService(_mockContext.Object, _cacheMock.Object, _bankAccountService, _configuration);
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
    [Test]
    public async Task UpdateUser_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User" };
        var userUpdateDto = new UserUpdateDto { Name = "Updated User" };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.UpdateUser(userId, userUpdateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.Name, Is.EqualTo(userUpdateDto.Name));
        });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public void UpdateUser_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userUpdateDto = new UserUpdateDto { Name = "Updated User" };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.UpdateUser(userId, userUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }

    
    //UPDATE PROFILE
    [Test]
    public async Task UpdateProfile_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User" };
        var userUpdateDto = new UserUpdateDto { Name = "Updated User", Email = "test@test.com", Username = "test", Dni = "54522318J",
            Gender = "Female", Password = "password", Avatar = "avatar.png", Phone = "123456789" };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.UpdateProfile(userId, userUpdateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<string>());
        });
    }
    
    [Test]
    public void UpdateProfile_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userUpdateDto = new UserUpdateDto { Name = "Updated User" };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.UpdateProfile(userId, userUpdateDto));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }
    
    //UPLOAD AVATAR
    [Test]
    public async Task UploadAvatar_UpdatesUserAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", Avatar = "oldAvatar.png" };
        var mockFile = new Mock<IFormFile>();
        var content = "dummy image content";
        var fileName = "newAvatar.png";
        var contentType = "image/png";
        var byteArray = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(byteArray);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.ContentType).Returns(contentType);
        mockFile.Setup(_ => _.Length).Returns(stream.Length);
        mockFile.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => stream.WriteAsync(byteArray, 0, byteArray.Length));

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.UploadAvatar(mockFile.Object, "http://localhost", userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Avatar, Is.EqualTo($"http://localhost/uploads/{userId}-newAvatar.png"));
        });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public void UploadAvatar_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockFile = new Mock<IFormFile>();

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.UploadAvatar(mockFile.Object, "http://localhost", userId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }
    
    [Test]
    public void UploadAvatar_InvalidFileType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User" };
        var mockFile = new Mock<IFormFile>();
        var content = "dummy image content";
        var fileName = "newAvatar.txt";
        var contentType = "text/plain";
        var byteArray = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(byteArray);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.ContentType).Returns(contentType);
        mockFile.Setup(_ => _.Length).Returns(stream.Length);
        mockFile.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => stream.WriteAsync(byteArray, 0, byteArray.Length));

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.UploadAvatar(mockFile.Object, "http://localhost", userId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Invalid file type. Only images are allowed"));
        });
    }
    
    [Test]
    public async Task UploadAvatar_UserWithDefaultAvatar_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", Avatar = User.ImageDefault };
        var mockFile = new Mock<IFormFile>();
        var content = "dummy image content";
        var fileName = "newAvatar.png";
        var contentType = "image/png";
        var byteArray = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(byteArray);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.ContentType).Returns(contentType);
        mockFile.Setup(_ => _.Length).Returns(stream.Length);
        mockFile.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => stream.WriteAsync(byteArray, 0, byteArray.Length));

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.UploadAvatar(mockFile.Object, "http://localhost", userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Avatar, Is.EqualTo($"http://localhost/uploads/{userId}-newAvatar.png"));
        });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task UploadAvatar_DirectoryNotExists_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", Avatar = User.ImageDefault };
        var mockFile = new Mock<IFormFile>();
        var content = "dummy image content";
        var fileName = "newAvatar.png";
        var contentType = "image/png";
        var byteArray = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(byteArray);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.ContentType).Returns(contentType);
        mockFile.Setup(_ => _.Length).Returns(stream.Length);
        mockFile.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken token) => stream.WriteAsync(byteArray, 0, byteArray.Length));

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.UploadAvatar(mockFile.Object, "http://localhost", userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Avatar, Is.EqualTo($"http://localhost/uploads/{userId}-newAvatar.png"));
        });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    //DELETE USER
    [Test]
    public async Task DeleteUser_ReturnsDeletedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User" };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _usersService.DeleteUser(userId);

        // Assert
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void DeleteUser_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync((User)null);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.DeleteUser(userId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }
    
    [Test]
    public async Task DeleteUser_UserWithAvatar_ReturnsDeletedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test User", Avatar = "avatar.png" };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

        _mockContext.Setup(x => x.Users).Returns(mockSet.Object);
        _mockContext.Setup(x => x.BankAccounts).ReturnsDbSet(new List<BankAccount>());
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _usersService.DeleteUser(userId);

        // Assert
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    //DELETE AVATAR
    [Test]
    public async Task DeleteAvatar_ReturnsUserWithDefaultAvatar()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Avatar = "avatar.png" };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _usersService.DeleteAvatar(userId);

        // Assert
        Assert.Multiple(() => { Assert.That(result.Avatar, Is.EqualTo(User.ImageDefault)); });

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void DeleteAvatar_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act
        var exception = Assert.ThrowsAsync<HttpException>(() => _usersService.DeleteAvatar(userId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Code, Is.EqualTo(404));
            Assert.That(exception.Message, Is.EqualTo("User not found"));
        });
    }
    
    //VALIDATE USER
    [Test]
    public async Task ValidateUserCredentials_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var username = "testUser";
        var password = "testPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User { Username = username, Password = hashedPassword };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act
        var result = await _usersService.ValidateUserCredentials(username, password);

        // Assert
        Assert.That(result, Is.EqualTo(user));
    }

    [Test]
    public void ValidateUserCredentials_InvalidUsername_ThrowsHttpException()
    {
        // Arrange
        var username = "invalidUser";
        var password = "testPassword";

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User>());

        // Act & Assert
        Assert.ThrowsAsync<HttpException>(() => _usersService.ValidateUserCredentials(username, password));
    }

    [Test]
    public void ValidateUserCredentials_InvalidPassword_ThrowsHttpException()
    {
        // Arrange
        var username = "testUser";
        var password = "invalidPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("testPassword");
        var user = new User { Username = username, Password = hashedPassword };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act & Assert
        Assert.ThrowsAsync<HttpException>(() => _usersService.ValidateUserCredentials(username, password));
    }
    
    //invalid password hash
    [Test]
    public void ValidateUserCredentials_InvalidPasswordHash_ThrowsHttpException()
    {
        // Arrange
        var username = "testUser";
        var password = "testPassword";
        var hashedPassword = "invalidHash";
        var user = new User { Username = username, Password = hashedPassword };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act & Assert
        Assert.ThrowsAsync<HttpException>(() => _usersService.ValidateUserCredentials(username, password));
    }
}