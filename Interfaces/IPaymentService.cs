
using TraceWebApi.Responses.Payments;
using TraceWebApi.Responses.Refunds;

namespace TraceWebApi.Interfaces;

public interface IPaymentService
{
    SessionResponse CreateCheckoutSession(decimal amount);
    RefundResponse Refund(string chargeId);
}
