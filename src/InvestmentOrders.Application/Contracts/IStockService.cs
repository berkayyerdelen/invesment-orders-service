using InvestmentOrders.Application.Responses;

namespace InvestmentOrders.Application.Contracts;

public interface IStockService
{
    Task<StockQuoteResponse> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);
}
