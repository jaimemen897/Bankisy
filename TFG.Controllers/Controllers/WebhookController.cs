using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using TFG.Context.DTOs.transactions;
using TFG.Controllers.DataAccessor;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController(IOptionsSnapshot<StripeSettings> stripeSettings, TransactionService transactionService) : Controller
{
    private readonly string _endpointSecret = stripeSettings.Value.EndpointSecret;

    //stripe listen --forward-to https://localhost:5196/webhook
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        Console.WriteLine(_endpointSecret);
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], _endpointSecret);

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                var ammount = paymentIntent.Amount / 100;
                var userId = Guid.Parse(paymentIntent.Metadata["userId"]);
                var iban = paymentIntent.Metadata["iban"];
                
                var incomeCreateDto = new IncomeCreateDto
                {
                    Amount = ammount,
                    IbanAccountDestination = iban
                };
                
                await transactionService.AddPaymentIntent(incomeCreateDto, userId);
            }

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new { error = e.Message });
        }
    }
}