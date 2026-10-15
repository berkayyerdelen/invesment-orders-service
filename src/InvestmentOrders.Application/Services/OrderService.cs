using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Application.Extensions.MappingExtensions;
using InvestmentOrders.Application.Requests;
using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Policies.Contracts;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Application.Services;

public sealed class OrderService(
    IAccountRepository accountRepository,
    IStockRepository stockRepository,
    IOrderRepository orderRepository,
    IOrderValidationPolicy validationPolicy,
    TimeProvider timeProvider) : IOrderService
{
    public async Task<OrderValidationResponse> ValidateOrderAsync(ValidateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var (account, stock, totalAmount) = await PriceOrderAsync(request.AccountId, request.Symbol, request.Quantity, cancellationToken);
        var isValid = validationPolicy.IsSatisfied(account, totalAmount, out var reason);
        return new OrderValidationResponse(isValid, stock.Price.Amount, totalAmount.Amount, reason);
    }

    public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        var (account, stock, totalAmount) = await PriceOrderAsync(request.AccountId, request.Symbol, request.Quantity, cancellationToken);

        // Atomic re-check-and-debit: the preview above can be stale by the time execution runs.
        await accountRepository.DebitAsync(account.Id, totalAmount, cancellationToken);

        var order = new Order(account.Id, stock.Symbol, request.Quantity, stock.Price, timeProvider.GetUtcNow());
        await orderRepository.AddAsync(order, cancellationToken);

        var updatedAccount = await accountRepository.GetByIdAsync(account.Id, cancellationToken) ?? account;
        return order.ToResponse(updatedAccount.CashBalance.Amount);
    }

    private async Task<(Account Account, Stock Stock, Money TotalAmount)> PriceOrderAsync(
        Guid accountId, string symbol, int quantity, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken)
            ?? throw new AccountNotFoundException(accountId);
        var stock = await stockRepository.FindBySymbolAsync(symbol, cancellationToken)
            ?? throw new StockNotFoundException(symbol);

        return (account, stock, stock.Price.Multiply(quantity));
    }
}
