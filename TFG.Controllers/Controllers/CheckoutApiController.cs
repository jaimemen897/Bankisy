using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace TFG.Controllers.Controllers;

[Route("create-checkout-session")]
[Authorize]
[ApiController]
public class CheckoutApiController : Controller
{
    public class AmountModel
    {
        public long Amount { get; set; }
    }
    
    [HttpPost]
    public ActionResult CreateCheckoutSession([FromBody] AmountModel amountModel)
    {
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