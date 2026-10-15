namespace InvestmentOrders.Api.Responses;

public sealed record StockQuoteApiResponse(string Symbol, string Name, decimal Price);
