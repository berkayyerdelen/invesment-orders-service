using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Models;

public sealed class Stock
{
    public Stock(string symbol, string name, Money price)
    {
        Symbol = symbol;
        Name = name;
        Price = price;
    }

    /// <summary>EF Core materialization only — owned-type navigations (<see cref="Price"/>) can't be bound through a constructor parameter.</summary>
    private Stock()
    {
    }

    public string Symbol { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
}
