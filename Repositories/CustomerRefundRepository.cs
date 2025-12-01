using TraceWebApi.EntityFrameworkCore;
using TraceWebApi.Interfaces;
using TraceWebApi.Models.Refunds;

namespace TraceWebApi.Repositories;

public class CustomerRefundRepository : Repository<CustomerRefund>, ICustomerRefundRepository
{
    public CustomerRefundRepository(TraceAppDbContext context) : base(context)
    {
    }
}
