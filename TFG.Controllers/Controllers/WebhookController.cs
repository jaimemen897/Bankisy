using Microsoft.AspNetCore.Mvc;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(WebhookService webhookService) : Controller
{
    //stripe listen --forward-to https://localhost:5196/webhook
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        return await webhookService.HandleWebhook(await new StreamReader(Request.Body).ReadToEndAsync(),
            Request.Headers["Stripe-Signature"]);
    }
}