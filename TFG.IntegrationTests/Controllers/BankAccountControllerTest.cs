using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using TFG.Context.DTOs.users;
using TFG.Services;

namespace TFG.IntegrationTests.Controllers;

public class BankAccountControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private string _token;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var sessionService = scope.ServiceProvider.GetService<SessionService>();
        var response = sessionService.Login(new UserLoginDto
        {
            Username = "admin",
            Password = "Admin1234"
        }).Result;

        // Parse the JSON response to get the token
        var jsonResponse = JObject.Parse(response);
        _token = jsonResponse["token"].ToString();

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
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
}