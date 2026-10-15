using System.Text.Json;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Domain.ValueObjects;
using InvestmentOrders.Infrastructure.Persistence;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace InvestmentOrders.Infrastructure.Repositories;

public sealed class StockRepository(InvestmentOrdersDbContext context, IConnectionMultiplexer redis, IDistributedLockProvider lockProvider) : IStockRepository
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

    public async Task<Stock?> FindBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.ToUpperInvariant();
        var cacheKey = $"stock:{normalizedSymbol}";
        var database = redis.GetDatabase();

        var cached = await database.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return Deserialize(cached!);
        }

        // Stampede protection: only one caller per symbol hits Postgres on a cache miss.
        await using var handle = await lockProvider.AcquireLockAsync($"stock-lock:{normalizedSymbol}", LockTimeout, cancellationToken);

        cached = await database.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return Deserialize(cached!);
        }

        var stock = await context.Stocks.FirstOrDefaultAsync(s => s.Symbol == normalizedSymbol, cancellationToken);
        if (stock is not null)
        {
            await database.StringSetAsync(cacheKey, Serialize(stock), CacheTtl);
        }

        return stock;
    }

    private static string Serialize(Stock stock) =>
        JsonSerializer.Serialize(new StockCacheEntry(stock.Symbol, stock.Name, stock.Price.Amount, stock.Price.Currency.Code));

    private static Stock Deserialize(string json)
    {
        var entry = JsonSerializer.Deserialize<StockCacheEntry>(json)!;
        return new Stock(entry.Symbol, entry.Name, new Money(entry.Amount, new Currency(entry.CurrencyCode)));
    }

    private sealed record StockCacheEntry(string Symbol, string Name, decimal Amount, string CurrencyCode);
}
