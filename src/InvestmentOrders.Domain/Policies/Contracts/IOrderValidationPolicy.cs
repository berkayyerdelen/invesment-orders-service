using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Policies.Contracts;

public interface IOrderValidationPolicy
{
    /// <summary>True when the order can be executed as-is; otherwise <paramref name="reason"/> explains why not.</summary>
    bool IsSatisfied(Account account, Money totalAmount, out string? reason);
}
