using Stripe;
using Stripe.Checkout;
using TraceWebApi.Interfaces;
using TraceWebApi.Responses.Payments;
using TraceWebApi.Responses.Refunds;


namespace TraceWebApi.Services.PaymentProvider;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;

    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
        var secretKey = _configuration["StripeSettings:SecretKey"]!;
        StripeConfiguration.ApiKey = secretKey;
    }

    public SessionResponse CreateCheckoutSession(decimal amount)
    {
        // This method creates a Stripe session
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100),  // Amount in cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Trace App Subscription",
                    },
                },
                Quantity = 1,
            },
        },
            Mode = "payment",
            SuccessUrl = _configuration["StripeSettings:SuccessUrl"]!,
            CancelUrl = _configuration["StripeSettings:CancelUrl"]!,
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return new SessionResponse
        {
            SessionId = session.Id,
            SessionUrl = session.Url,
            SuccessUrl = session.SuccessUrl,
            CancelUrl= session.CancelUrl,
        };
    }
    public RefundResponse Refund(string chargeId)
    {
        var refundService = new RefundService();

        Refund refund = refundService.Create(new RefundCreateOptions
        {
            Charge = chargeId
        });

        return new RefundResponse
        {
            RefundId = refund.Id,
            Amount = refund.Amount / 100m,
        };
    }
}
