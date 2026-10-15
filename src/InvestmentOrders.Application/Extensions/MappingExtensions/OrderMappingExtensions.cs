using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Models;

namespace InvestmentOrders.Application.Extensions.MappingExtensions;

public static class OrderMappingExtensions
{
    public static OrderResponse ToResponse(this Order order, decimal remainingCashBalance) => new(
        order.Id,
        order.Symbol,
        order.Quantity,
        order.PricePerShare.Amount,
        order.TotalAmount.Amount,
        order.ExecutedAt,
        remainingCashBalance);
}
