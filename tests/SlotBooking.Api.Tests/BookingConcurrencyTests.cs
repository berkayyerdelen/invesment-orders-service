using SlotBooking.Api.Domain;
using SlotBooking.Api.Services;
using Xunit;

namespace SlotBooking.Api.Tests;

/// <summary>
/// Race-condition tests against a real PostgreSQL. Each concurrent request uses its own DbContext,
/// the same way separate HTTP requests do in production.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class BookingConcurrencyTests(PostgresFixture postgres)
{
    private static readonly DateTimeOffset SlotStart = new(DateTime.UtcNow.Date.AddDays(30).AddHours(9), TimeSpan.Zero);

    [Fact]
    public async Task ConcurrentCreates_ForSameSlot_OnlyOneSucceeds()
    {
        var roomId = await postgres.RoomIdAsync("Room B");
        var start = SlotStart;

        var attempts = Enumerable.Range(0, 10).Select(async index =>
        {
            await using var scope = postgres.NewScope();
            var service = scope.ServiceProvider.GetRequiredService<BookingService>();
            try
            {
                await service.CreateAsync(new CreateBookingCommand(roomId, start, start.AddHours(1), $"Guest {index}"), CancellationToken.None);
                return true;
            }
            catch (ConflictException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(attempts);

        Assert.Equal(1, results.Count(success => success));
    }

    [Fact]
    public async Task ConcurrentOverlappingCreates_AcceptOnlyNonOverlapping()
    {
        var roomId = await postgres.RoomIdAsync("Meeting Room 1");
        var start = SlotStart.AddDays(1);

        // Two overlapping (10-11 and 10:30-11:30) and one back-to-back (11-12) booking, all at once.
        var requests = new[]
        {
            new CreateBookingCommand(roomId, start, start.AddHours(1), "Overlap A"),
            new CreateBookingCommand(roomId, start.AddMinutes(30), start.AddMinutes(90), "Overlap B"),
            new CreateBookingCommand(roomId, start.AddHours(1), start.AddHours(2), "Back to back"),
        };

        var results = await Task.WhenAll(requests.Select(async command =>
        {
            await using var scope = postgres.NewScope();
            try
            {
                await scope.ServiceProvider.GetRequiredService<BookingService>().CreateAsync(command, CancellationToken.None);
                return command.GuestName;
            }
            catch (ConflictException)
            {
                return null;
            }
        }));

        var accepted = results.Where(name => name is not null).ToHashSet();
        Assert.Contains("Back to back", accepted);
        Assert.False(accepted.Contains("Overlap A") && accepted.Contains("Overlap B"));
    }

    [Fact]
    public async Task StaleUpdate_IsRejectedWithCurrentVersion()
    {
        var roomId = await postgres.RoomIdAsync("Room A");
        var start = SlotStart.AddDays(2);

        Booking created;
        await using (var scope = postgres.NewScope())
        {
            created = await scope.ServiceProvider.GetRequiredService<BookingService>()
                .CreateAsync(new CreateBookingCommand(roomId, start, start.AddHours(1), "Original"), CancellationToken.None);
        }

        // User 1 and user 2 both read the booking at version V.
        var readVersion = created.Version;

        await using (var scope = postgres.NewScope())
        {
            await scope.ServiceProvider.GetRequiredService<BookingService>().UpdateAsync(
                created.Id,
                new UpdateBookingCommand(readVersion, start, start.AddHours(1), "User 1 edit"),
                CancellationToken.None);
        }

        await using var staleScope = postgres.NewScope();
        var conflict = await Assert.ThrowsAsync<ConflictException>(() =>
            staleScope.ServiceProvider.GetRequiredService<BookingService>().UpdateAsync(
                created.Id,
                new UpdateBookingCommand(readVersion, start, start.AddHours(1), "User 2 edit"),
                CancellationToken.None));

        Assert.NotNull(conflict.CurrentVersion);
        Assert.NotEqual(readVersion, conflict.CurrentVersion);
    }

    [Fact]
    public async Task ConcurrentCancels_WithSameVersion_OnlyOneSucceeds()
    {
        var roomId = await postgres.RoomIdAsync("Room B");
        var start = SlotStart.AddDays(3);

        Booking created;
        await using (var scope = postgres.NewScope())
        {
            created = await scope.ServiceProvider.GetRequiredService<BookingService>()
                .CreateAsync(new CreateBookingCommand(roomId, start, start.AddHours(1), "Cancel race"), CancellationToken.None);
        }

        var version = created.Version;
        var attempts = Enumerable.Range(0, 5).Select(async _ =>
        {
            await using var scope = postgres.NewScope();
            try
            {
                await scope.ServiceProvider.GetRequiredService<BookingService>()
                    .CancelAsync(created.Id, version, CancellationToken.None);
                return true;
            }
            catch (ConflictException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(attempts);

        Assert.Equal(1, results.Count(success => success));
    }

    [Fact]
    public async Task RescheduleIntoTakenSlot_IsRejected()
    {
        var roomId = await postgres.RoomIdAsync("Room A");
        var start = SlotStart.AddDays(4);

        Booking first, second;
        await using (var scope = postgres.NewScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<BookingService>();
            first = await service.CreateAsync(new CreateBookingCommand(roomId, start, start.AddHours(1), "First"), CancellationToken.None);
            second = await service.CreateAsync(new CreateBookingCommand(roomId, start.AddHours(2), start.AddHours(3), "Second"), CancellationToken.None);
        }

        await using var updateScope = postgres.NewScope();
        await Assert.ThrowsAsync<ConflictException>(() =>
            updateScope.ServiceProvider.GetRequiredService<BookingService>().UpdateAsync(
                second.Id,
                new UpdateBookingCommand(second.Version, start, start.AddHours(1), "Second moved onto First"),
                CancellationToken.None));
    }
}
