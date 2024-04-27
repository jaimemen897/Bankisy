using Microsoft.AspNetCore.Mvc;
using Stripe;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(IndexService indexService) : Controller
{
    private const string EndpointSecret = "whsec_c8ea8f06c1f738abf20901f88e119db5d264856e45901ed14be89f10347ddcd1";

    //stripe listen --forward-to localhost:5196/webhook
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], EndpointSecret);

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                var ammount = paymentIntent.Amount / 100;
                var userId = Guid.Parse(paymentIntent.Metadata["userId"]);
                await indexService.AddPaymentIntent(ammount, userId);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }
}