using Stripe.Checkout;
using TFG.Context.DTOs.transactions;
using SessionServiceStripe = Stripe.Checkout.SessionService;

namespace TFG.Services;

public class CheckoutService(SessionService sessionService)
{
    public Session CreateCheckoutSession(AmountModel amountModel)
    {
        var user = sessionService.GetMyself().Result;
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = amountModel.Amount * 100,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Stubborn Attachments"
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = "https://localhost:44464/",
            CancelUrl = "https://localhost:44464/deposit",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", user.Id.ToString() },
                    { "iban", amountModel.Iban }
                }
            }
        };

        var service = new SessionServiceStripe();
        return service.Create(options);
    }
}