using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;

namespace TFG.IntegrationTests.Controllers;

public class CardControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _token;
    private Guid _userId;
    private string _cardNumber;

    [SetUp]
    public void Setup()
    {
        var builder = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                             typeof(DbContextOptions<BankContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<BankContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });
                });
            });
        _factory = builder;
        _client = _factory.CreateClient();
        var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BankContext>();
        var userService = scope.ServiceProvider.GetService<UsersService>();

        // Create the user
        var user = new User
        {
            Id = Guid.Parse("19d3c1ec-3449-4d95-ac69-f7e847025b23"),
            Dni = "12345678A",
            Email = "email@email.com",
            Name = "Name",
            Phone = "123456789",
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
        };
        context.Users.Add(user);

        var bankAccount = new BankAccount
        {
            Iban = "ES7921000813610123456789",
            Users = [user],
            Balance = 1000,
            IsDeleted = false,
            AcceptBizum = false,
            AccountType = AccountType.Current,
            TransactionsDestination = [],
            TransactionsOrigin = [],
            Cards = []
        };

        var bankAccount2 = new BankAccount
        {
            Iban = "ES7921000813610123456780",
            Users = [user],
            Balance = 1000,
            IsDeleted = false,
            AcceptBizum = false,
            AccountType = AccountType.Current,
            TransactionsDestination = [],
            TransactionsOrigin = [],
            Cards = []
        };
        context.BankAccounts.Add(bankAccount);
        context.BankAccounts.Add(bankAccount2);

        // Create the card
        var card = new Card
        {
            CardNumber = "1234567812345678",
            IsBlocked = false,
            IsDeleted = false,
            BankAccount = bankAccount,
            User = user,
            Cvv = "XJkhKugRJOCeY6kKZvVOGg==",
            Pin = "P3/lf1dk7rzW3jkWnWTGNg=="
        };
        context.Cards.Add(card);

        context.SaveChanges();

        var sessionService = scope.ServiceProvider.GetService<SessionService>();
        var response = sessionService.Login(new UserLoginDto
        {
            Username = "admin",
            Password = "Admin1234"
        }).Result;

        var jsonResponse = JObject.Parse(response);
        _token = jsonResponse["token"].ToString();

        _userId = Guid.Parse("19d3c1ec-3449-4d95-ac69-f7e847025b23");
        _cardNumber = "1234567812345678";

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    [TearDown]
    public void TearDown()
    {
        using var scope = _factory.Services.GetService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BankContext>();
        context.Database.EnsureDeleted();

        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetCards_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Card");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetMyCards_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Card/my-cards");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetCardByCardNumber_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.GetAsync($"/Card/{cardNumber}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task CreateCard_ReturnsExpectedResult()
    {
        var card = new CardCreateDto
        {
            Pin = "1234",
            CardType = "Debit",
            UserId = _userId,
            BankAccountIban = "ES7921000813610123456780"
        };

        var content = new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Card", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task CreateCardForMySelf_ReturnsExpectedResult()
    {
        // Arrange
        var card = new CardCreateDto
        {
            Pin = "1234",
            CardType = "Debit",
            BankAccountIban = "ES7921000813610123456780",
            UserId = _userId
        };
        var content = new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Card/my-card", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;
        var card = new CardUpdateDto
        {
            Pin = "2943",
            UserId = _userId,
            BankAccountIban = "ES7921000813610123456780",
            CardType = "Debit"
        };
        var content = new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/Card/{cardNumber}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;
        var card = new CardUpdateDto
        {
            Pin = "2943",
            BankAccountIban = "ES7921000813610123456780",
            CardType = "Debit"
        };
        var content = new StringContent(JsonConvert.SerializeObject(card), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/Card/my-card/{cardNumber}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task DeleteCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.DeleteAsync($"/Card/{cardNumber}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.DeleteAsync($"/Card/my-card/{cardNumber}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RenovateCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/{cardNumber}/renovate", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RenovateMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/my-card/{cardNumber}/renovate", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task BlockCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/{cardNumber}/block", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnblockCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Block the card first
        var blockResponse = await _client.PostAsync($"/Card/{cardNumber}/block", null);
        Assert.That(blockResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Act
        var response = await _client.PostAsync($"/Card/{cardNumber}/unblock", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task BlockMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/my-card/{cardNumber}/block", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnblockMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Block the card first
        var blockResponse = await _client.PostAsync($"/Card/my-card/{cardNumber}/block", null);
        Assert.That(blockResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Act
        var response = await _client.PostAsync($"/Card/my-card/{cardNumber}/unblock", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ActivateCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/{cardNumber}/activate", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ActivateMyCard_ReturnsExpectedResult()
    {
        // Arrange
        var cardNumber = _cardNumber;

        // Act
        var response = await _client.PostAsync($"/Card/my-card/{cardNumber}/activate", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}