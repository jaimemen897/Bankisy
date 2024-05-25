using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SessionService = TFG.Services.SessionService;
using SessionServiceStripe = Stripe.Checkout.SessionService;

namespace TFG.Controllers.Controllers;

[Route("create-checkout-session")]
[Authorize]
[ApiController]
public class CheckoutApiController(SessionService sessionService) : Controller
{
    [HttpPost]
    public ActionResult CreateCheckoutSession([FromBody] AmountModel amountModel)
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
        var session = service.Create(options);

        return Json(new { id = session.Id });
    }

    [HttpPost("create-connect-account")]
    public ActionResult CreateConnectAccount()
    {
        var options = new AccountCreateOptions
        {
            Type = "standard"
        };

        var service = new AccountService();
        var account = service.Create(options);

        return Json(new { accountId = account.Id });
    }

    [HttpPost("transfer-funds")]
    public ActionResult TransferFunds(string accountId, long amount)
    {
        var options = new TransferCreateOptions
        {
            Amount = amount,
            Currency = "usd",
            Destination = accountId
        };

        var service = new TransferService();
        var transfer = service.Create(options);

        return Json(new { transferId = transfer.Id });
    }

    public class AmountModel
    {
        public long Amount { get; set; }
        public string Iban { get; set; }
    }
}