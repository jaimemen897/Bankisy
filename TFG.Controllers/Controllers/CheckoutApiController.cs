using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using TFG.Services;
using SessionService = Stripe.Checkout.SessionService;

namespace TFG.Controllers.Controllers;

[Route("create-checkout-session")]
[Authorize]
[ApiController]
public class CheckoutApiController(IndexService indexService) : Controller
{
    public class AmountModel
    {
        public long Amount { get; set; }
    }
    
    [HttpPost]
    public ActionResult CreateCheckoutSession([FromBody] AmountModel amountModel)
    {
        var user = indexService.GetMyself();
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string>
            {
                "card",
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = amountModel.Amount * 100,
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Stubborn Attachments",
                        },
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = "https://localhost:44464/",
            CancelUrl = "https://localhost:44464/deposit",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", user.Id.ToString() }
                }
            }
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Json(new { id = session.Id });
    }

    [HttpPost("create-connect-account")]
    public ActionResult CreateConnectAccount()
    {
        var options = new AccountCreateOptions
        {
            Type = "standard",
        };

        var service = new AccountService();
        Account account = service.Create(options);

        return Json(new { accountId = account.Id });
    }

    [HttpPost("transfer-funds")]
    public ActionResult TransferFunds(string accountId, long amount)
    {
        var options = new TransferCreateOptions
        {
            Amount = amount,
            Currency = "usd",
            Destination = accountId,
        };

        var service = new TransferService();
        Transfer transfer = service.Create(options);

        return Json(new { transferId = transfer.Id });
    }
}