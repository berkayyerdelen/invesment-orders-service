using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlotBooking.Api.Data;
using SlotBooking.Api.Services;
using Testcontainers.PostgreSql;
using Xunit;

namespace SlotBooking.Api.Tests;

/// <summary>Starts one PostgreSQL container for the test run and applies the real migrations to it.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public ServiceProvider Services { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(ConnectionString));
        services.AddScoped<BookingService>();
        services.AddSingleton(TimeProvider.System);
        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DatabaseMigrator.MigrateAsync(db, scope.ServiceProvider.GetRequiredService<ILogger<PostgresFixture>>());
    }

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>A short-lived scope with its own DbContext, as each HTTP request gets in production.</summary>
    public AsyncServiceScope NewScope() => Services.CreateAsyncScope();

    public async Task<Guid> RoomIdAsync(string name)
    {
        await using var scope = NewScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Rooms.Where(room => room.Name == name).Select(room => room.Id).SingleAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
