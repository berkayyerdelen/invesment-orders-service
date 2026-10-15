using System.Globalization;
using InvestmentOrders.Domain.Exceptions;

namespace InvestmentOrders.Domain.ValueObjects;

public sealed record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0m, currency);

    public Money Add(Money other) => Combine(other, (a, b) => a + b);

    public Money Subtract(Money other) => Combine(other, (a, b) => a - b);

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    public static bool operator >(Money left, Money right) => Compare(left, right) > 0;

    public static bool operator <(Money left, Money right) => Compare(left, right) < 0;

    public static bool operator >=(Money left, Money right) => Compare(left, right) >= 0;

    public static bool operator <=(Money left, Money right) => Compare(left, right) <= 0;

    public override string ToString() => $"${Amount.ToString("F2", CultureInfo.InvariantCulture)}";

    private Money Combine(Money other, Func<decimal, decimal, decimal> operation)
    {
        EnsureSameCurrency(this, other);
        return new Money(operation(Amount, other.Amount), Currency);
    }

    private static int Compare(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount.CompareTo(right.Amount);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new CurrencyMismatchException(left.Currency, right.Currency);
        }
    }
}
