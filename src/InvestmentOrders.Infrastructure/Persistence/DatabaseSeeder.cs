using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;
using InvestmentOrders.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace InvestmentOrders.Infrastructure.Persistence;

/// <summary>Inserts the fixed demo account and stock on first run. Called once after migrations are applied.</summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(InvestmentOrdersDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Accounts.AnyAsync(cancellationToken))
        {
            context.Accounts.Add(new Account(SeedData.DefaultAccountId, new Money(SeedData.DefaultCashBalance, Currency.Usd)));
        }

        if (!await context.Stocks.AnyAsync(cancellationToken))
        {
            context.Stocks.Add(new Stock(SeedData.AaplSymbol, SeedData.AaplName, new Money(SeedData.AaplPrice, Currency.Usd)));
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
