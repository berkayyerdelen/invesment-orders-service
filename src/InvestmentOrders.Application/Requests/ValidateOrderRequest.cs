namespace InvestmentOrders.Application.Requests;

public sealed record ValidateOrderRequest(Guid AccountId, string Symbol, int Quantity);
