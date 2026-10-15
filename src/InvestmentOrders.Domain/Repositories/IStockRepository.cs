using InvestmentOrders.Domain.Models;

namespace InvestmentOrders.Domain.Repositories;

public interface IStockRepository
{
    Task<Stock?> FindBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
