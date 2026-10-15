using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Policies.Contracts;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Policies;

public sealed class SufficientFundsPolicy : IOrderValidationPolicy
{
    public bool IsSatisfied(Account account, Money totalAmount, out string? reason)
    {
        if (totalAmount > account.CashBalance)
        {
            reason = $"Order total {totalAmount} exceeds available cash balance {account.CashBalance}.";
            return false;
        }

        reason = null;
        return true;
    }
}
