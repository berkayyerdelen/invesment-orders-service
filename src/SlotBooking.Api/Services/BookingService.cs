using Microsoft.EntityFrameworkCore;
using SlotBooking.Api.Data;
using SlotBooking.Api.Domain;
using SlotBooking.Api.Errors;
using SlotBooking.Api.Models;

namespace SlotBooking.Api.Services;

public sealed record CreateBookingCommand(Guid RoomId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string GuestName);

/// <summary>Carries the version the client last saw, so the write can be checked against the database.</summary>
public sealed record UpdateBookingCommand(uint Version, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string GuestName);

/// <summary>
/// Booking use cases. Concurrency is handled in two layers:
/// <list type="bullet">
///   <item>Double booking: the database EXCLUDE constraint rejects overlapping active bookings, even under races.
///   The service never relies on a "check then insert" sequence that two requests could both pass.</item>
///   <item>Lost updates: writes carry the version (xmin) the client read, and the UPDATE only matches that row
///   version. A stale write fails with 409 and the client reloads.</item>
/// </list>
/// </summary>
public sealed class BookingService(AppDbContext db, TimeProvider timeProvider, ILogger<BookingService> logger)
{
    private const string SlotTakenMessage = "The room is already booked for part of this time range.";

    public async Task<IReadOnlyList<Booking>> ListForRoomAsync(Guid roomId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        if (to <= from)
        {
            throw new ValidationException("The 'to' value must be after 'from'.");
        }

        return await db.Bookings
            .AsNoTracking()
            .Include(booking => booking.Room)
            .Where(booking => booking.RoomId == roomId && booking.StartsAt < to && booking.EndsAt > from)
            .OrderBy(booking => booking.StartsAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Booking> GetAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return await FindAsReadOnlyAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");
    }

    public async Task<Booking> CreateAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var roomExists = await db.Rooms.AnyAsync(room => room.Id == command.RoomId, cancellationToken);
        if (!roomExists)
        {
            throw new NotFoundException("Room not found.");
        }

        var booking = Booking.Create(command.RoomId, command.StartsAt, command.EndsAt, command.GuestName, timeProvider.GetUtcNow());
        db.Bookings.Add(booking);

        await SaveAsync(booking.Id, cancellationToken);

        logger.LogInformation("Booking {BookingId} created for room {RoomId}", booking.Id, booking.RoomId);
        return await GetAsync(booking.Id, cancellationToken);
    }

    public async Task<Booking> UpdateAsync(Guid bookingId, UpdateBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await FindTrackedAsync(bookingId, cancellationToken);

        // Tell EF to match the row version the client read, not the one just loaded.
        db.Entry(booking).Property(entity => entity.Version).OriginalValue = command.Version;
        booking.Reschedule(command.StartsAt, command.EndsAt, command.GuestName);

        await SaveAsync(bookingId, cancellationToken);

        logger.LogInformation("Booking {BookingId} rescheduled", bookingId);
        return await GetAsync(bookingId, cancellationToken);
    }

    public async Task<Booking> CancelAsync(Guid bookingId, uint version, CancellationToken cancellationToken)
    {
        var booking = await FindTrackedAsync(bookingId, cancellationToken);

        db.Entry(booking).Property(entity => entity.Version).OriginalValue = version;
        booking.Cancel();

        await SaveAsync(bookingId, cancellationToken);

        logger.LogInformation("Booking {BookingId} cancelled", bookingId);
        return await GetAsync(bookingId, cancellationToken);
    }

    private async Task<Booking> FindTrackedAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return await db.Bookings.Include(booking => booking.Room)
                   .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken)
               ?? throw new NotFoundException("Booking not found.");
    }

    private async Task<Booking?> FindAsReadOnlyAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return await db.Bookings.AsNoTracking()
            .Include(booking => booking.Room)
            .FirstOrDefaultAsync(booking => booking.Id == bookingId, cancellationToken);
    }

    /// <summary>
    /// Saves and translates the two expected database failures into domain conflicts.
    /// The caller never has to reason about SQLSTATE codes.
    /// </summary>
    private async Task SaveAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The row changed (or disappeared) since the client read it. Report the current version
            // so the client can reload and decide what to do.
            var currentVersion = await db.Bookings.AsNoTracking()
                .Where(booking => booking.Id == bookingId)
                .Select(booking => (uint?)booking.Version)
                .FirstOrDefaultAsync(cancellationToken);

            logger.LogWarning("Optimistic concurrency conflict on booking {BookingId}", bookingId);

            if (currentVersion is null)
            {
                throw new NotFoundException("Booking not found.");
            }

            throw new ConflictException(
                "The booking was changed by someone else. Reload it and try again.",
                currentVersion);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsExclusionViolation(exception))
        {
            logger.LogInformation("Double booking prevented for booking {BookingId}", bookingId);
            throw new ConflictException(SlotTakenMessage);
        }
    }
}
