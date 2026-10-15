using InvestmentOrders.SharedKernel.Exceptions;

namespace InvestmentOrders.Domain.Exceptions;

public sealed class StockNotFoundException(string symbol)
    : DomainException("Stock not found", $"No stock found with symbol '{symbol}'.", 404);
