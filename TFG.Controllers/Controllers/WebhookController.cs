using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace TFG.Controllers.Controllers;

[Route("webhook")]
[ApiController]
public class WebhookController : Controller
{
    // This is your Stripe CLI webhook secret for testing your endpoint locally.
    const string endpointSecret = "whsec_c8ea8f06c1f738abf20901f88e119db5d264856e45901ed14be89f10347ddcd1";

    //stripe listen --forward-to localhost:5196/webhook
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);

            // Handle the event
            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                Console.WriteLine("PaymentIntent was successful!");
            }
            else if (stripeEvent.Type == Events.PaymentMethodAttached)
            {
                var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                Console.WriteLine("PaymentMethod was attached to a Customer!");
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