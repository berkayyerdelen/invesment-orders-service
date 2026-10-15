using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestmentOrders.Infrastructure.Configurations;

public sealed class InvestmentAccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.OwnsOne(a => a.CashBalance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("CashBalance").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, code => new Currency(code))
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
        builder.Navigation(a => a.CashBalance).IsRequired();
    }
}
