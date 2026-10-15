namespace InvestmentOrders.Api.Responses;

public sealed record OrderApiResponse(
    Guid OrderId,
    string Symbol,
    int Quantity,
    decimal PricePerShare,
    decimal TotalAmount,
    DateTimeOffset ExecutedAt,
    decimal RemainingCashBalance);
