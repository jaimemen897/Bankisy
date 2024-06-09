using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.IntegrationTests.Controllers;

public class TransactionControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _token;

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
        var user2 = new User
        {
            Id = Guid.Parse("19d3c1ec-3449-4d95-ac69-f7e847025b24"),
            Dni = "12345678A",
            Email = "email@email.com",
            Name = "Name",
            Phone = "123456780",
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin1234"),
        };
        context.Users.Add(user);
        context.Users.Add(user2);

        var bankAccount = new BankAccount
        {
            Iban = "ES7921000813610123456789",
            Users = [user],
            Balance = 1000,
            IsDeleted = false,
            AcceptBizum = true,
            AccountType = AccountType.Current,
            TransactionsDestination = [],
            TransactionsOrigin = [],
            Cards = []
        };

        var bankAccount2 = new BankAccount
        {
            Iban = "ES7921000813610123456780",
            Users = [user2],
            Balance = 1000,
            IsDeleted = false,
            AcceptBizum = true,
            AccountType = AccountType.Current,
            TransactionsDestination = [],
            TransactionsOrigin = [],
            Cards = []
        };

        context.BankAccounts.Add(bankAccount);
        context.BankAccounts.Add(bankAccount2);

        var transaction = new Transaction
        {
            Id = 1,
            Amount = 100,
            Concept = "Concept",
            Date = DateTime.Now,
            IbanAccountDestination = "ES7921000813610123456789",
            IbanAccountOrigin = "ES7921000813610123456780",
            BankAccountDestinationIban = bankAccount,
            BankAccountOriginIban = bankAccount2
        };
        context.Transactions.Add(transaction);

        context.SaveChanges();

        var sessionService = scope.ServiceProvider.GetService<SessionService>();
        var response = sessionService.Login(new UserLoginDto
        {
            Username = "admin",
            Password = "Admin1234"
        }).Result;

        var jsonResponse = JObject.Parse(response);
        _token = jsonResponse["token"].ToString();

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
    public async Task GetTransactions_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Transactions");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var transactions = JsonConvert.DeserializeObject<Pagination<TransactionResponseDto>>(responseContent);

        Assert.IsNotNull(transactions);
        Assert.IsNotEmpty(transactions.Items);
    }

    [Test]
    public async Task GetTransaction_ReturnsExpectedResult()
    {
        // Arrange
        var transactionId = 1; // Replace with a valid transaction ID

        // Act
        var response = await _client.GetAsync($"/Transactions/{transactionId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var transaction = JsonConvert.DeserializeObject<TransactionResponseDto>(responseContent);

        Assert.IsNotNull(transaction);
    }

    [Test]
    public async Task GetTransactionsForAccount_ReturnsExpectedResult()
    {
        // Arrange
        var bankAccountIban = "ES7921000813610123456789"; // Replace with a valid IBAN

        // Act
        var response = await _client.GetAsync($"/Transactions/{bankAccountIban}/transactions");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var transactions = JsonConvert.DeserializeObject<List<TransactionResponseDto>>(responseContent);

        Assert.IsNotNull(transactions);
        Assert.IsNotEmpty(transactions);
    }

    [Test]
    public async Task GetTransactionsByIban_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7921000813610123456789"; // Replace with a valid IBAN

        // Act
        var response = await _client.GetAsync($"/Transactions/bankaccount/{iban}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var transactions = JsonConvert.DeserializeObject<List<TransactionResponseDto>>(responseContent);

        Assert.IsNotNull(transactions);
        Assert.IsNotEmpty(transactions);
    }

    [Test]
    public async Task CreateBizum_ReturnsExpectedResult()
    {
        // Arrange
        var bizum = new BizumCreateDto
        {
            Amount = 100,
            Concept = "Concept",
            PhoneNumberUserOrigin = "123456789",
            PhoneNumberUserDestination = "123456780"
        };
        var content = new StringContent(JsonConvert.SerializeObject(bizum), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Transactions/bizum", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();
        var bizumResponse = JsonConvert.DeserializeObject<BizumResponseDto>(responseContent);

        Assert.IsNotNull(bizumResponse);
    }

    [Test]
    public async Task GetMyTransactions_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Transactions/myself");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var transactions = JsonConvert.DeserializeObject<Pagination<TransactionResponseDto>>(responseContent);

        Assert.IsNotNull(transactions);
        Assert.IsNotEmpty(transactions.Items);
    }

    [Test]
    public async Task GetSummary_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Transactions/summary");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var summary = JsonConvert.DeserializeObject<UserSummary>(responseContent);

        Assert.IsNotNull(summary);
    }

    [Test]
    public async Task CreateTransaction_ReturnsExpectedResult()
    {
        // Arrange
        var transaction = new TransactionCreateDto
        {
            Amount = 100,
            Concept = "Concept",
            IbanAccountDestination = "ES7921000813610123456780",
            IbanAccountOrigin = "ES7921000813610123456789"
        };
        var content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Transactions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task DeleteTransaction_ReturnsExpectedResult()
    {
        // Arrange
        var id = 1; // Replace with a valid transaction ID

        // Act
        var response = await _client.DeleteAsync($"/Transactions/{id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}