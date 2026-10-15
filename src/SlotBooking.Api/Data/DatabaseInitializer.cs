using Microsoft.EntityFrameworkCore;
using SlotBooking.Api.Models;

namespace SlotBooking.Api.Data;

/// <summary>
/// Creates the schema and the database-level rules that EF Core cannot express:
/// no two active bookings may overlap in the same room. The exclusion constraint is the
/// final guard against double booking, so it holds even when two requests race.
/// </summary>
public static class DatabaseInitializer
{
    private const string DatabaseRulesSql = """
        CREATE EXTENSION IF NOT EXISTS btree_gist;

        DO $$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'bookings_time_range_valid') THEN
                ALTER TABLE bookings
                    ADD CONSTRAINT bookings_time_range_valid CHECK (ends_at > starts_at);
            END IF;

            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'bookings_no_overlap') THEN
                ALTER TABLE bookings
                    ADD CONSTRAINT bookings_no_overlap
                    EXCLUDE USING gist (room_id WITH =, tstzrange(starts_at, ends_at, '[)') WITH &&)
                    WHERE (status = 'Active');
            END IF;
        END
        $$;
        """;

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync(DatabaseRulesSql, cancellationToken);

        if (!await db.Rooms.AnyAsync(cancellationToken))
        {
            db.Rooms.AddRange(
                new Room { Id = Guid.NewGuid(), Name = "Room A" },
                new Room { Id = Guid.NewGuid(), Name = "Room B" },
                new Room { Id = Guid.NewGuid(), Name = "Meeting Room 1" });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded default rooms");
        }

        logger.LogInformation("Database schema and rules are in place");
    }
}
