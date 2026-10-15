using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Policies;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Tests;

public class SufficientFundsPolicyTests
{
    private readonly SufficientFundsPolicy policy = new();

    [Fact]
    public void IsSatisfied_ReturnsTrue_WhenTotalDoesNotExceedBalance()
    {
        var account = new Account(Guid.NewGuid(), new Money(1_000m, Currency.Usd));

        var isSatisfied = policy.IsSatisfied(account, new Money(1_000m, Currency.Usd), out var reason);

        Assert.True(isSatisfied);
        Assert.Null(reason);
    }

    [Fact]
    public void IsSatisfied_ReturnsFalse_WhenTotalExceedsBalance()
    {
        var account = new Account(Guid.NewGuid(), new Money(100m, Currency.Usd));

        var isSatisfied = policy.IsSatisfied(account, new Money(100.01m, Currency.Usd), out var reason);

        Assert.False(isSatisfied);
        Assert.NotNull(reason);
    }
}
