namespace InvestmentOrders.Application.Requests;

public sealed record PlaceOrderRequest(Guid AccountId, string Symbol, int Quantity, string IdempotencyKey);
