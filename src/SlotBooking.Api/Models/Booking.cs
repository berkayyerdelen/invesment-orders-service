using SlotBooking.Api.Domain;

namespace SlotBooking.Api.Models;

public enum BookingStatus
{
    Active,
    Cancelled
}

/// <summary>
/// A reservation of a room for a time range. Business rules live here so every caller
/// (API, background jobs, tests) gets the same validation.
/// </summary>
public sealed class Booking
{
    public const int MaxGuestNameLength = 150;
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

    private Booking()
    {
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Room? Room { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string GuestName { get; private set; } = string.Empty;
    public BookingStatus Status { get; private set; } = BookingStatus.Active;

    /// <summary>
    /// Optimistic concurrency token, backed by the PostgreSQL xmin system column.
    /// Every UPDATE checks it, so a stale write fails instead of silently overwriting newer data.
    /// </summary>
    public uint Version { get; private set; }

    public static Booking Create(Guid roomId, DateTimeOffset startsAt, DateTimeOffset endsAt, string guestName, DateTimeOffset now)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
        };

        if (startsAt < now.AddMinutes(-5))
        {
            throw new ValidationException("The booking must start in the future.");
        }

        booking.ApplySchedule(startsAt, endsAt, guestName);
        return booking;
    }

    public void Reschedule(DateTimeOffset startsAt, DateTimeOffset endsAt, string guestName)
    {
        EnsureActive();
        ApplySchedule(startsAt, endsAt, guestName);
    }

    public void Cancel()
    {
        EnsureActive();
        Status = BookingStatus.Cancelled;
    }

    private void ApplySchedule(DateTimeOffset startsAt, DateTimeOffset endsAt, string guestName)
    {
        if (endsAt <= startsAt)
        {
            throw new ValidationException("The end time must be after the start time.");
        }

        if (endsAt - startsAt > MaxDuration)
        {
            throw new ValidationException($"A booking cannot be longer than {MaxDuration.TotalHours} hours.");
        }

        if (string.IsNullOrWhiteSpace(guestName) || guestName.Length > MaxGuestNameLength)
        {
            throw new ValidationException($"The guest name is required and must be at most {MaxGuestNameLength} characters.");
        }

        StartsAt = startsAt;
        EndsAt = endsAt;
        GuestName = guestName.Trim();
    }

    private void EnsureActive()
    {
        if (Status != BookingStatus.Active)
        {
            throw new ConflictException("The booking is already cancelled.");
        }
    }
}
