using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void TotalAmount_IsQuantityTimesPricePerShare()
    {
        var order = new Order(Guid.NewGuid(), "AAPL", 10, new Money(150m, Currency.Usd), DateTimeOffset.UtcNow);

        Assert.Equal(new Money(1_500m, Currency.Usd), order.TotalAmount);
    }
}
