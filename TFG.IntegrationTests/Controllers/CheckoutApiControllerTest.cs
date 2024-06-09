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

namespace TFG.IntegrationTests.Controllers;

public class CheckoutApiControllerTest
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
    public async Task CreateCheckoutSession_ReturnsExpectedResult()
    {
        // Arrange
        var amountModel = new AmountModel
        {
            Amount = 100,
            Iban = "ES7357810375677605638221"
        };
        var content = new StringContent(JsonConvert.SerializeObject(amountModel), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/create-checkout-session", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }
}