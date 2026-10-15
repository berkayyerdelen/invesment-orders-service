using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentOrders.Infrastructure.Configurations;

public sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(s => s.Symbol);
        builder.Property(s => s.Symbol).HasMaxLength(10);
        builder.Property(s => s.Name).HasMaxLength(200);

        builder.OwnsOne(s => s.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Price").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, code => new Currency(code))
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
        builder.Navigation(s => s.Price).IsRequired();
    }
}
