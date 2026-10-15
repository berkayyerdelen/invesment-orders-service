namespace InvestmentOrders.Application.Responses;

public sealed record StockQuoteResponse(string Symbol, string Name, decimal Price);
