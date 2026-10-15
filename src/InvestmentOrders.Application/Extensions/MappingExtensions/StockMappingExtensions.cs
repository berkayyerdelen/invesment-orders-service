using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Models;

namespace InvestmentOrders.Application.Extensions.MappingExtensions;

public static class StockMappingExtensions
{
    public static StockQuoteResponse ToResponse(this Stock stock) => new(stock.Symbol, stock.Name, stock.Price.Amount);
}
