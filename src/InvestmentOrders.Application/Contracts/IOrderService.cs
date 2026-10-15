using InvestmentOrders.Application.Requests;
using InvestmentOrders.Application.Responses;

namespace InvestmentOrders.Application.Contracts;

public interface IOrderService
{
    /// <summary>Pure preview: prices the order and checks funds, never persists anything.</summary>
    Task<OrderValidationResponse> ValidateOrderAsync(ValidateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>Debits the account and persists the order.</summary>
    Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default);
}
