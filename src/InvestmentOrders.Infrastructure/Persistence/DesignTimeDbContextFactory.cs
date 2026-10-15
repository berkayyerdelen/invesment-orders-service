using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvestmentOrders.Infrastructure.Persistence;

/// <summary>Used only by the `dotnet ef` CLI to generate/apply migrations at design time. The real runtime connection string comes from Aspire via the Api project.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InvestmentOrdersDbContext>
{
    public InvestmentOrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvestmentOrdersDbContext>()
            .UseNpgsql("Host=localhost;Database=investmentorders;Username=postgres;Password=postgres");

        return new InvestmentOrdersDbContext(optionsBuilder.Options);
    }
}
