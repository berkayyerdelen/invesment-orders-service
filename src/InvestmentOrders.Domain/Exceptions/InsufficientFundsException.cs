using InvestmentOrders.Domain.ValueObjects;
using InvestmentOrders.SharedKernel.Exceptions;

namespace InvestmentOrders.Domain.Exceptions;

public sealed class InsufficientFundsException(Money requestedAmount, Money availableBalance)
    : DomainException(
        "Insufficient funds",
        $"Order total {requestedAmount} exceeds available cash balance {availableBalance}.",
        422)
{
    public Money RequestedAmount { get; } = requestedAmount;
    public Money AvailableBalance { get; } = availableBalance;
}
