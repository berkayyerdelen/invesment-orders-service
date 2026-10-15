using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestmentOrders.Infrastructure.Repositories;

public sealed class OrderRepository(InvestmentOrdersDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
}
