using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;

namespace TFG.ServicesTests;

[TestFixture]
public class UserServiceTest
{
    private BankContext _bankContext;
    private Mock<IMemoryCache> _cacheMock;
    private UsersService _usersService;

    [SetUp]
    public void SetUp()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankContext>();
        optionsBuilder.UseInMemoryDatabase("BankTest");
        
        _bankContext = new BankContext(optionsBuilder.Options);
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _usersService = new UsersService(_bankContext, _cacheMock.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
        _bankContext.Dispose();
    }

    [Test]
    public async Task GetUserAsync_ReturnsExpectedUser()
    {
        // Arrange
        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User"
        };

        var mockSet = new Mock<DbSet<User>>();
        mockSet.Setup(x => x.FindAsync(expectedUser.Id)).ReturnsAsync(expectedUser);

        var options = new DbContextOptionsBuilder<BankContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var mockContext = new BankContext(options);
        mockContext.Users = mockSet.Object;

        _usersService = new UsersService(mockContext, _cacheMock.Object);

        // Act
        var result = await _usersService.GetUserAsync(expectedUser.Id);

        // Assert
        Assert.That(result.Id, Is.EqualTo(expectedUser.Id));
    }
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