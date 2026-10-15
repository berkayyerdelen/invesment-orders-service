using InvestmentOrders.Domain.ValueObjects;
using InvestmentOrders.SharedKernel.Exceptions;

namespace InvestmentOrders.Domain.Exceptions;

public sealed class CurrencyMismatchException(Currency expected, Currency actual)
    : DomainException(
        "Currency mismatch",
        $"Cannot combine amounts in '{expected}' and '{actual}'.",
        422);
