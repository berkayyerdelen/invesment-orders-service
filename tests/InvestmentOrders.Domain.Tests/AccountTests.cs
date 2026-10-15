using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Debit_ReducesBalance_WhenFundsAreSufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(500m, Currency.Usd));

        account.Debit(new Money(200m, Currency.Usd));

        Assert.Equal(new Money(300m, Currency.Usd), account.CashBalance);
    }

    [Fact]
    public void Debit_Throws_AndLeavesBalanceUnchanged_WhenFundsAreInsufficient()
    {
        var account = new Account(Guid.NewGuid(), new Money(100m, Currency.Usd));

        var exception = Assert.Throws<InsufficientFundsException>(() => account.Debit(new Money(100.01m, Currency.Usd)));

        Assert.Equal(422, exception.StatusCode);
        Assert.Equal(new Money(100m, Currency.Usd), account.CashBalance);
    }
}
