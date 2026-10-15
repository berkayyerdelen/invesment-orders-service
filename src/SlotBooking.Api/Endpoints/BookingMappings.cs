using SlotBooking.Api.Models;

namespace SlotBooking.Api.Endpoints;

public static class BookingMappings
{
    public static BookingResponse ToResponse(this Booking booking) => new(
        booking.Id,
        booking.RoomId,
        booking.Room?.Name ?? string.Empty,
        booking.StartsAt,
        booking.EndsAt,
        booking.GuestName,
        booking.Status.ToString(),
        booking.Version);
}
