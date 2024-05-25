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

namespace TFG.ServicesTests.Session;

[TestFixture]
public class SessionServiceTest
{
    private Mock<IMemoryCache> _cacheMock;
    private Mock<BankContext> _mockContext;

    private SessionService _sessionService;
    private UsersService _usersService;
    private BankAccountService _bankAccountService;
    private CardService _cardService;

    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    private IConfiguration _configuration;

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

        var configurationMock = new Mock<IConfiguration>();
        _configuration = configurationMock.Object;

        _usersService = new UsersService(_mockContext.Object, _cacheMock.Object, _bankAccountService, _configuration);
        _sessionService = new SessionService(_usersService, _mockHttpContextAccessor.Object, _configuration);
    }
}
















