using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TFG.Context.DTOs.transactions;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("[controller]")]
public class WebhookController (IndexService indexService) : Controller
{
    const string EndpointSecret = "whsec_c8ea8f06c1f738abf20901f88e119db5d264856e45901ed14be89f10347ddcd1";

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
                PaymentIntent paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                var ammount = paymentIntent.Amount / 100;
                var userId = Guid.Parse(paymentIntent.Metadata["userId"]);
                await indexService.AddPaymentIntent(ammount, userId);
            }
            else
            {
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }
}