using Stripe;

namespace TFG.Services;

public class StripeService
{
    public StripeService()
    {
        StripeConfiguration.ApiKey = "sk_test_51P7eS8D74icxIHcU4kn0dVmFuoZQhnf4gbAydb4NTzXzfI0oJTFjliD1H46CNyf2yrBuon0v3RwcHpJiUGkOZTYB00btmbH4Ic";
    }
    
    public string CreateCharge(string source, int amount)
    {
        var options = new ChargeCreateOptions
        {
            Amount = amount,
            Currency = "eur",
            Source = source,
            Description = "Deposit to bank account"
        };

        var service = new ChargeService();
        Charge charge = service.Create(options);

        return charge.Id;
    }
}