using InvestmentOrders.Domain.Models;
using InvestmentOrders.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace InvestmentOrders.Infrastructure.Persistence;

public sealed class InvestmentOrdersDbContext(DbContextOptions<InvestmentOrdersDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InvestmentAccountConfiguration());
        modelBuilder.ApplyConfiguration(new StockConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }
}
