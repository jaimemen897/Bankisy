using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TFG.Context.Context;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.IntegrationTests.Controllers;

public class UsersControllerTest
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
    public async Task GetUsers_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Users");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<Pagination<UserResponseDto>>(responseContent);

        Assert.IsNotNull(users);
        Assert.IsNotEmpty(users.Items);
    }

    [Test]
    public async Task GetUser_ReturnsExpectedResult()
    {
        // Arrange
        var userId = _userId;

        // Act
        var response = await _client.GetAsync($"/Users/{userId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task GetAllUsers_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.GetAsync("/Users/all");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(content);
    }

    [Test]
    public async Task CreateUser_ReturnsExpectedResult()
    {
        // Arrange
        var user = new UserCreateDto
        {
            Username = "test",
            Password = "Test1235",
            Name = "Test",
            Dni = "12345679B",
            Email = "email@email2.com",
            Phone = "123456789",
            Avatar = "avatar.jpg",
            Gender = "Male",
        };
        var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Users", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateUser_ReturnsExpectedResult()
    {
        // Arrange
        var id = _userId;
        var user = new UserUpdateDto
        {
            Username = "test",
            Password = "Test1234",
            Name = "Test",
            Dni = "12345678B",
            Email = "email@email.com",
            Phone = "123456789",
            Avatar = "avatar.jpg",
            Gender = "Male",
        };
        var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/Users/{id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateProfile_ReturnsExpectedResult()
    {
        // Arrange
        var user = new UserUpdateDto
        {
            // Fill with appropriate test data
        };
        var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/Users/profile", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task UpdateUserAvatar_ReturnsExpectedResult()
    {
        // Arrange
        var id = _userId;
        var fileName = "avatar.jpg";
        var contentType = "image/jpeg";

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(new byte[0]);
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);
        fileMock.Setup(_ => _.ContentType).Returns(contentType);

        var fileContent = new StreamContent(fileMock.Object.OpenReadStream())
        {
            Headers =
            {
                ContentLength = fileMock.Object.Length,
                ContentType = new MediaTypeHeaderValue(contentType)
            }
        };

        var content = new MultipartFormDataContent();
        content.Add(fileContent, "avatar", fileMock.Object.FileName);

        // Act
        var response = await _client.PutAsync($"/Users/{id}/avatar", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task DeleteUserAvatar_ReturnsExpectedResult()
    {
        // Arrange
        var id = _userId;

        // Act
        var response = await _client.DeleteAsync($"/Users/{id}/avatar");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }

    [Test]
    public async Task DeleteUser_ReturnsExpectedResult()
    {
        // Arrange
        var id = _userId;

        // Act
        var response = await _client.DeleteAsync($"/Users/{id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
    
    [Test]
    public async Task UpdateAvatar_ReturnsExpectedResult()
    {
        // Arrange
        var fileName = "avatar.jpg";
        var contentType = "image/jpeg";

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(new byte[0]);
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);
        fileMock.Setup(_ => _.ContentType).Returns(contentType);

        var fileContent = new StreamContent(fileMock.Object.OpenReadStream())
        {
            Headers =
            {
                ContentLength = fileMock.Object.Length,
                ContentType = new MediaTypeHeaderValue(contentType)
            }
        };

        var content = new MultipartFormDataContent();
        content.Add(fileContent, "avatar", fileMock.Object.FileName);

        // Act
        var response = await _client.PutAsync("/Users/avatar", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }
    
    [Test]
    public async Task DeleteMyAvatar_ReturnsExpectedResult()
    {
        // Act
        var response = await _client.DeleteAsync("/Users/avatar");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
    
}