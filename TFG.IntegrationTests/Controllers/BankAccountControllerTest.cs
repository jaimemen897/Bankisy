using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;

namespace TFG.IntegrationTests.Controllers;

public class BankAccountControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _token;
    private Guid _userId;

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

        // Create the bank account
        var bankAccount = new BankAccount
        {
            Iban = "ES7357810375677605638221",
            AcceptBizum = true,
            Users = [user],
        };
        context.BankAccounts.Add(bankAccount);

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
    public async Task GetBankAccounts_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/BankAccounts");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetBankAccountsByMySelf_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/BankAccounts/my-self/user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetBankAccount_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";

        // Act
        var response = await _client.GetAsync($"/BankAccounts/{iban}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetBankAccountByUserId_ReturnsExpectedResult()
    {
        // Arrange
        var userId = _userId;

        // Act
        var response = await _client.GetAsync($"/BankAccounts/user/{userId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task CreateBankAccount_ReturnsExpectedResult()
    {
        // Arrange
        var bankAccount = new BankAccountCreateDto
        {
            AcceptBizum = false,
            AccountType = "Saving",
            UsersId = [_userId],
        };
        var content = new StringContent(JsonConvert.SerializeObject(bankAccount), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/BankAccounts", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task CreateBankAccountForMySelf_ReturnsExpectedResult()
    {
        // Arrange
        var bankAccount = new BankAccountCreateDto
        {
            AcceptBizum = false,
            AccountType = "Saving",
            UsersId = [_userId],
        };
        var content = new StringContent(JsonConvert.SerializeObject(bankAccount), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/BankAccounts/my-self", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateBankAccount_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";
        var bankAccount = new BankAccountUpdateDto
        {
            AcceptBizum = false,
            AccountType = "Saving",
            UsersId = [_userId],
        };
        var content = new StringContent(JsonConvert.SerializeObject(bankAccount), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/BankAccounts/{iban}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task ActiveBizum_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";
        var userId = _userId;

        // Act
        var response = await _client.PostAsync($"/BankAccounts/{iban}/active-bizum/{userId}", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ActiveBizumForMySelf_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";

        // Act
        var response = await _client.PutAsync($"/BankAccounts/my-self/{iban}/active-bizum", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteBankAccount_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";

        // Act
        var response = await _client.DeleteAsync($"/BankAccounts/{iban}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task ActiveBankAccount_ReturnsExpectedResult()
    {
        // Arrange
        var iban = "ES7357810375677605638221";

        // Act
        var response = await _client.PutAsync($"/BankAccounts/{iban}/active", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}