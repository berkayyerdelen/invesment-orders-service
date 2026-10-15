using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SlotBooking.Api.Data;

/// <summary>
/// Applies the embedded SQL migrations in order, each in its own transaction, and records them
/// in schema_migrations. A PostgreSQL advisory lock serialises concurrent startups (for example
/// several API replicas starting together), so two instances never apply the same migration.
/// </summary>
public static class DatabaseMigrator
{
    private const long MigrationLockKey = 727_274_001;

    public static async Task MigrateAsync(AppDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        var migrations = GetMigrations();

        await using var connection = new NpgsqlConnection(db.Database.GetConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await Execute(connection, transaction, "SELECT pg_advisory_xact_lock(@key)", new NpgsqlParameter("key", MigrationLockKey), cancellationToken);
        await Execute(connection, transaction, """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                id         text        PRIMARY KEY,
                applied_at timestamptz NOT NULL DEFAULT now()
            )
            """, cancellationToken: cancellationToken);

        foreach (var (id, sql) in migrations)
        {
            await using var check = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM schema_migrations WHERE id = @id)", connection, transaction);
            check.Parameters.AddWithValue("id", id);
            if ((bool)(await check.ExecuteScalarAsync(cancellationToken))!)
            {
                continue;
            }

            logger.LogInformation("Applying database migration {MigrationId}", id);
            await Execute(connection, transaction, sql, cancellationToken: cancellationToken);
            await Execute(connection, transaction, "INSERT INTO schema_migrations (id) VALUES (@id)",
                new NpgsqlParameter("id", id), cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Database is up to date ({MigrationCount} migrations)", migrations.Count);
    }

    /// <summary>Returns embedded migrations ordered by file name, e.g. 001_initial_schema.sql.</summary>
    public static IReadOnlyList<(string Id, string Sql)> GetMigrations()
    {
        var assembly = typeof(DatabaseMigrator).Assembly;
        const string prefix = "SlotBooking.Api.Data.Migrations.";

        return assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name =>
            {
                using var stream = assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidOperationException($"Missing embedded migration {name}");
                using var reader = new StreamReader(stream);
                return (Id: name[prefix.Length..], Sql: reader.ReadToEnd());
            })
            .ToList();
    }

    private static async Task Execute(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql,
        NpgsqlParameter? parameter = null, CancellationToken cancellationToken = default)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        if (parameter is not null)
        {
            command.Parameters.Add(parameter);
        }
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
