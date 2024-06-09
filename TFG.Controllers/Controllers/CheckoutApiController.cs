using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using SessionService = TFG.Services.SessionService;
using SessionServiceStripe = Stripe.Checkout.SessionService;

namespace TFG.Controllers.Controllers;

[Route("create-checkout-session")]
[Authorize]
[ApiController]
public class CheckoutApiController(CheckoutService checkoutService) : Controller
{
    [HttpPost]
    public ActionResult CreateCheckoutSession([FromBody] AmountModel amountModel)
    {
        var session = checkoutService.CreateCheckoutSession(amountModel);
        return Json(new { id = session.Id });
    }
}