namespace InvestmentOrders.Application.Responses;

public sealed record OrderResponse(
    Guid OrderId,
    string Symbol,
    int Quantity,
    decimal PricePerShare,
    decimal TotalAmount,
    DateTimeOffset ExecutedAt,
    decimal RemainingCashBalance);
