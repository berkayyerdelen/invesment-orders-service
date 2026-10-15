using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Models;

public sealed class Account
{
    public Account(Guid id, Money cashBalance)
    {
        Id = id;
        CashBalance = cashBalance;
    }

    /// <summary>EF Core materialization only — owned-type navigations (<see cref="CashBalance"/>) can't be bound through a constructor parameter.</summary>
    private Account()
    {
    }

    public Guid Id { get; private set; }
    public Money CashBalance { get; private set; } = null!;

    /// <summary>Enforces the "never go negative" invariant on the actual state change (the validation policy is only a preview).</summary>
    public void Debit(Money amount)
    {
        if (amount > CashBalance)
        {
            throw new InsufficientFundsException(amount, CashBalance);
        }

        CashBalance = CashBalance.Subtract(amount);
    }
}
