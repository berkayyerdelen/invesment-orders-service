namespace InvestmentOrders.Domain.ValueObjects;

public sealed record Currency
{
    public static readonly Currency Usd = new("USD");

    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
        {
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(code));
        }

        Code = code.ToUpperInvariant();
    }

    public override string ToString() => Code;
}
