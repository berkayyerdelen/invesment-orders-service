using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;

namespace InvestmentOrders.Application.Tests.Fakes;

internal sealed class FakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } = [];

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var result = Orders.FirstOrDefault(o => o.Id == orderId);
        return Task.FromResult(result);
    }
}
