using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Application.Services;
using InvestmentOrders.Domain.Policies;
using InvestmentOrders.Domain.Policies.Contracts;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Infrastructure.Repositories;
using Medallion.Threading;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace InvestmentOrders.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvestmentOrdersServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddSingleton<IOrderValidationPolicy, SufficientFundsPolicy>();
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IDistributedLockProvider>(sp =>
            new RedisDistributedSynchronizationProvider(sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
