using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.EntityFrameworkCore;
using Newtonsoft.Json;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Exceptions;

namespace TFG.ServicesTests.Session;

[TestFixture]
public class SessionServiceTest
{
    private Mock<IMemoryCache> _cacheMock;
    private Mock<BankContext> _mockContext;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<IConfiguration> configurationMock = new();

    private SessionService _sessionService;
    private UsersService _usersService;
    private BankAccountService _bankAccountService;
    private CardService _cardService;
    
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BankContext>().UseInMemoryDatabase("TestDatabase").Options;
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        _mockContext = new Mock<BankContext>(options);
        _cardService = new CardService(_mockContext.Object);
        _bankAccountService = new BankAccountService(_mockContext.Object, _cacheMock.Object, _cardService);

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        configurationMock.Setup(x => x["Jwt:Key"]).Returns("a_very_long_and_secure_key");
        configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
        configurationMock.Setup(x => x["Jwt:Audience"]).Returns("audience");
        configurationMock.Setup(x => x["Jwt:secret"]).Returns("a_very_long_and_secure_key_laskdjñflaksjdf");

        _usersService = new UsersService(_mockContext.Object, _cacheMock.Object, _bankAccountService,
            configurationMock.Object);
        _sessionService = new SessionService(_usersService, _mockHttpContextAccessor.Object, configurationMock.Object);
    }

    //LOGIN
    [Test]
    public async Task Login_ReturnsExpectedToken()
    {
        // Arrange
        var userLoginDto = new UserLoginDto { Username = "testUser", Password = "testPassword" };
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.Admin, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword")
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act
        var result = await _sessionService.Login(userLoginDto);

        // Assert
        var tokenObject = JsonConvert.DeserializeObject<dynamic>(result);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken((string)tokenObject.token);

        Assert.Multiple(() =>
        {
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Is.EqualTo(user.Id.ToString()));
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value, Is.EqualTo(user.Email));
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                Is.EqualTo(user.Role.ToString()));

            Assert.That((string)tokenObject.user.Id, Is.EqualTo(user.Id.ToString()));
            Assert.That((string)tokenObject.user.Email, Is.EqualTo(user.Email));
            Assert.That((string)tokenObject.user.Role, Is.EqualTo("0"));
            Assert.That((string)tokenObject.user.Name, Is.EqualTo(user.Name));
        });
    }

    [Test]
    public void Login_InvalidCredentials_ThrowsHttpException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.Admin, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword")
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        // Act & Assert
        Assert.ThrowsAsync<HttpException>(() =>
            _sessionService.Login(new UserLoginDto { Username = "testUser", Password = "wrongPassword" }));
    }


    //REGISTER
    [Test]
    public async Task Register_ReturnsExpectedToken()
    {
        // Arrange
        var userRegisterDto = new UserCreateDto
        {
            Username = "testUser", Password = "testPassword", Email = "test@example.com", Name = "Test User",
            Gender = "Male", Dni = "12345678A", Phone = "123456789"
        };
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.User, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword"), Dni = "12345678A",
            Phone = "123456787"
        };

        _mockContext.Setup(x => x.Users).ReturnsDbSet([]);

        // Act
        var result = await _sessionService.Register(userRegisterDto);

        // Assert
        var tokenObject = JsonConvert.DeserializeObject<dynamic>(result);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken((string)tokenObject.token);

        Assert.Multiple(() =>
        {
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value, Is.EqualTo(user.Email));
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                Is.EqualTo(user.Role.ToString()));

            Assert.That((string)tokenObject.user.Email, Is.EqualTo(user.Email));
            Assert.That((string)tokenObject.user.Role, Is.EqualTo("1"));
            Assert.That((string)tokenObject.user.Name, Is.EqualTo(user.Name));
        });
    }
    
    [Test]
    public void Register_UserAlreadyExists_ThrowsHttpException()
    {
        // Arrange
        var userRegisterDto = new UserCreateDto
        {
            Username = "testUser", Password = "testPassword", Email = "test@example.com", Name = "Test User",
            Gender = "Male", Dni = "12345678A", Phone = "123456789"
        };
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.User, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword"), Dni = "12345678A",
            Phone = "123456787"
        };
        
        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        
        // Act & Assert
        Assert.ThrowsAsync<HttpException>(() => _sessionService.Register(userRegisterDto));
    }
    
    
    //GET USER BY TOKEN
    [Test]
    public async Task GetUserByToken_ReturnsExpectedUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.User, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword"), Dni = "12345678A",
            Phone = "123456787"
        };
        
        var tokenJson = SessionService.GetToken(user, configurationMock.Object);
        var tokenObject = JsonConvert.DeserializeObject<dynamic>(tokenJson);
        var token = (string)tokenObject.token;
        
        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });
        
        // Act
        var result = await _sessionService.GetUserByToken(token);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Role, Is.EqualTo(user.Role.ToString()));
            Assert.That(result.Name, Is.EqualTo(user.Name));
        });
    }

    [Test]
    public void GetUserByToken_InvalidToken_ThrowsHttpException()
    {
        // Act & Assert
        Assert.ThrowsAsync<SecurityTokenMalformedException>(() => _sessionService.GetUserByToken("invalidToken"));
    }
    
    
    //GET MYSELF
    [Test]
    public async Task GetMyself_ReturnsExpectedUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.User, Name = "Test User",
            Username = "testUser", Password = BCrypt.Net.BCrypt.HashPassword("testPassword"), Dni = "12345678A",
            Phone = "123456787"
        };

        var tokenJson = SessionService.GetToken(user, configurationMock.Object);
        var tokenObject = JsonConvert.DeserializeObject<dynamic>(tokenJson);
        var token = (string)tokenObject.token;

        _mockContext.Setup(x => x.Users).ReturnsDbSet(new List<User> { user });

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {token}";
        _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        // Act
        var result = await _sessionService.GetMyself();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Role, Is.EqualTo(user.Role.ToString()));
            Assert.That(result.Name, Is.EqualTo(user.Name));
        });
    }
    

    //GET TOKEN
    [Test]
    public void GetToken_ReturnsExpectedToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Role = Roles.Admin, Name = "Test User" };
        var result = SessionService.GetToken(user, configurationMock.Object);

        // Assert
        var tokenObject = JsonConvert.DeserializeObject<dynamic>(result);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken((string)tokenObject.token);

        Assert.Multiple(() =>
        {
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Is.EqualTo(user.Id.ToString()));
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value, Is.EqualTo(user.Email));
            Assert.That(token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                Is.EqualTo(user.Role.ToString()));

            Assert.That((string)tokenObject.user.Id, Is.EqualTo(user.Id.ToString()));
            Assert.That((string)tokenObject.user.Email, Is.EqualTo(user.Email));
            Assert.That((string)tokenObject.user.Role, Is.EqualTo("0"));
            Assert.That((string)tokenObject.user.Name, Is.EqualTo(user.Name));
        });
    }
}