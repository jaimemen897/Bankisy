using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.stripe;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class StripeController(StripeService stripeService) : ControllerBase
{
    [HttpPost("deposit")]
    public IActionResult Deposit([FromBody] DepositCreateDto request)
    {
        try
        {
            var chargeId = stripeService.CreateCharge(request.Source, request.Amount);

            return Ok(new { ChargeId = chargeId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}