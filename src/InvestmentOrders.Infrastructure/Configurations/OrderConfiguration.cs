using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentOrders.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Symbol).HasMaxLength(10);
        builder.Ignore(o => o.TotalAmount);

        builder.OwnsOne(o => o.PricePerShare, money =>
        {
            money.Property(m => m.Amount).HasColumnName("PricePerShare").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, code => new Currency(code))
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
        builder.Navigation(o => o.PricePerShare).IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
