using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;

namespace InvestmentOrders.Application.Tests.Fakes;

internal sealed class FakeStockRepository(Stock stock) : IStockRepository
{
    public Task<Stock?> FindBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var result = string.Equals(stock.Symbol, symbol, StringComparison.OrdinalIgnoreCase) ? stock : null;
        return Task.FromResult(result);
    }
}
