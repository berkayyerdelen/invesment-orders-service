using InvestmentOrders.Application.Requests;
using InvestmentOrders.Application.Services;
using InvestmentOrders.Application.Tests.Fakes;
using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Policies;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Application.Tests;

public class OrderServiceTests
{
    private static OrderService CreateService(Account account, Stock stock, FakeOrderRepository orderRepository) =>
        new(
            new FakeAccountRepository(account),
            new FakeStockRepository(stock),
            orderRepository,
            new SufficientFundsPolicy(),
            TimeProvider.System);

    [Fact]
    public async Task ValidateOrderAsync_ReturnsValid_WhenFundsAreSufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(10_000m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var service = CreateService(account, stock, new FakeOrderRepository());

        var result = await service.ValidateOrderAsync(new ValidateOrderRequest(account.Id, "AAPL", 10));

        Assert.True(result.IsValid);
        Assert.Equal(1_500m, result.TotalAmount);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateOrderAsync_ReturnsInvalid_WhenFundsAreInsufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(100m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var service = CreateService(account, stock, new FakeOrderRepository());

        var result = await service.ValidateOrderAsync(new ValidateOrderRequest(account.Id, "AAPL", 10));

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateOrderAsync_Throws_WhenStockIsUnknown()
    {
        var account = new Account(Guid.NewGuid(), new Money(10_000m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var service = CreateService(account, stock, new FakeOrderRepository());

        await Assert.ThrowsAsync<StockNotFoundException>(
            () => service.ValidateOrderAsync(new ValidateOrderRequest(account.Id, "MSFT", 1)));
    }

    [Fact]
    public async Task ValidateOrderAsync_Throws_WhenAccountIsUnknown()
    {
        var account = new Account(Guid.NewGuid(), new Money(10_000m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var service = CreateService(account, stock, new FakeOrderRepository());

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => service.ValidateOrderAsync(new ValidateOrderRequest(Guid.NewGuid(), "AAPL", 1)));
    }

    [Fact]
    public async Task PlaceOrderAsync_DebitsAccountAndPersistsOrder_WhenFundsAreSufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(10_000m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var orderRepository = new FakeOrderRepository();
        var service = CreateService(account, stock, orderRepository);

        var result = await service.PlaceOrderAsync(new PlaceOrderRequest(account.Id, "AAPL", 10));

        Assert.Equal(1_500m, result.TotalAmount);
        Assert.Equal(8_500m, result.RemainingCashBalance);
        Assert.Equal(new Money(8_500m, Currency.Usd), account.CashBalance);
        Assert.Single(orderRepository.Orders);
    }

    [Fact]
    public async Task PlaceOrderAsync_Throws_AndDoesNotPersist_WhenFundsAreInsufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(100m, Currency.Usd));
        var stock = new Stock("AAPL", "Apple Inc.", new Money(150m, Currency.Usd));
        var orderRepository = new FakeOrderRepository();
        var service = CreateService(account, stock, orderRepository);

        await Assert.ThrowsAsync<InsufficientFundsException>(
            () => service.PlaceOrderAsync(new PlaceOrderRequest(account.Id, "AAPL", 10)));

        Assert.Equal(new Money(100m, Currency.Usd), account.CashBalance);
        Assert.Empty(orderRepository.Orders);
    }
}
