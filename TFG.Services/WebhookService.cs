using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using TFG.Context.DataAccessor;
using TFG.Context.DTOs.transactions;

namespace TFG.Services;

public class WebhookService(IOptionsSnapshot<StripeSettings> stripeSettings, TransactionService transactionService)
{
    private readonly string _endpointSecret = stripeSettings.Value.EndpointSecret;

    public async Task<IActionResult> HandleWebhook(string json, string stripeSignature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _endpointSecret);

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

            return new OkResult();
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new { error = e.Message });
        }
    }
}