using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Application.Extensions.MappingExtensions;
using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Repositories;

namespace InvestmentOrders.Application.Services;

public sealed class StockService(IStockRepository stockRepository) : IStockService
{
    public async Task<StockQuoteResponse> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var stock = await stockRepository.FindBySymbolAsync(symbol, cancellationToken)
            ?? throw new StockNotFoundException(symbol);

        return stock.ToResponse();
    }
}
