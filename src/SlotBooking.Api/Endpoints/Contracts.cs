namespace SlotBooking.Api.Endpoints;

public sealed record RoomResponse(Guid Id, string Name);

public sealed record BookingResponse(
    Guid Id,
    Guid RoomId,
    string RoomName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string GuestName,
    string Status,
    uint Version);

public sealed record CreateBookingRequest(Guid RoomId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string GuestName);

public sealed record UpdateBookingRequest(uint Version, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string GuestName);

public sealed record CancelBookingRequest(uint Version);
