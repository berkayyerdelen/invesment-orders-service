using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Models;

public sealed class Order
{
    public Order(Guid accountId, string symbol, int quantity, Money pricePerShare, DateTimeOffset executedAt, string idempotencyKey)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Symbol = symbol;
        Quantity = quantity;
        PricePerShare = pricePerShare;
        ExecutedAt = executedAt;
        IdempotencyKey = idempotencyKey;
    }

    /// <summary>EF Core materialization only — owned-type navigations (<see cref="PricePerShare"/>) can't be bound through a constructor parameter.</summary>
    private Order()
    {
    }

    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Symbol { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money PricePerShare { get; private set; } = null!;
    public Money TotalAmount => PricePerShare.Multiply(Quantity);
    public DateTimeOffset ExecutedAt { get; private set; }

    /// <summary>Client-supplied token that dedupes a retried order-placement request. Unique per order.</summary>
    public string IdempotencyKey { get; private set; } = null!;
}
