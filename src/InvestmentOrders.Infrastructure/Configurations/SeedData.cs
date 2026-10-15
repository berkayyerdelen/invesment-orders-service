namespace InvestmentOrders.Infrastructure.Configurations;

/// <summary>Fixed single-account, single-stock demo data (the assignment only needs an in-memory store).</summary>
public static class SeedData
{
    public static readonly Guid DefaultAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public const decimal DefaultCashBalance = 10_000.00m;

    public const string AaplSymbol = "AAPL";
    public const string AaplName = "Apple Inc.";
    public const decimal AaplPrice = 150.00m;
}
