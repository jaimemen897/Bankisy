using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TFG.Context.Context;
using TFG.Services;

namespace TFG.IntegrationTests.Controllers;

public class WebhookControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

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
    public async Task Index_ReturnsBadRequest_WhenStripeSignatureIsInvalid()
    {
        // Arrange
        var webhookData = new
        {
            id = "evt_1PPn5mD74icxIHcUmOiUGXi7",
            eventObject = "event",
            api_version = "2024-04-10",
            created = 1717944694,
            data = new
            {
                dataObject = new
                {
                    id = "cs_test_a12c1jranMCvDOMK6RwMfTh2gwS8x4StRuuul1UHhlHOX1M4kNz9nn5NjA",
                    checkoutSession = "checkout.session",
                    // ... include all other properties here ...
                    status = "complete",
                    submit_type = (string)null,
                    subscription = (string)null,
                    success_url = "https://localhost:44464/",
                    total_details = new
                    {
                        amount_discount = 0,
                        amount_shipping = 0,
                        amount_tax = 0
                    },
                    ui_mode = "hosted",
                    url = (string)null
                }
            },
            livemode = false,
            pending_webhooks = 2,
            request = new
            {
                id = (string)null,
                idempotency_key = (string)null
            },
            type = "checkout.session.completed"
        };

        var content = new StringContent(JsonConvert.SerializeObject(webhookData), Encoding.UTF8, "application/json");

        _client.DefaultRequestHeaders.Add("Stripe-Signature",
            "t=1717945202,v1=2a33c17e2014f34b41739e1e57b47d41a794823b17329782899d75b528a5fee4,v0=381689585c695dceeb0840c23a3603ba83c2fa39cfd901f164ed224f75a06749");

        // Act
        var response = await _client.PostAsync("/Webhook", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.IsNotNull(responseContent);
    }
}