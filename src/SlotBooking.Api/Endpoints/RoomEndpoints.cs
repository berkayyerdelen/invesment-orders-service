using Microsoft.EntityFrameworkCore;
using SlotBooking.Api.Data;
using SlotBooking.Api.Services;

namespace SlotBooking.Api.Endpoints;

public static class RoomEndpoints
{
    public static RouteGroupBuilder MapRoomEndpoints(this RouteGroupBuilder api)
    {
        var rooms = api.MapGroup("/rooms").WithTags("Rooms");

        rooms.MapGet("/", async (AppDbContext db, CancellationToken cancellationToken) =>
            await db.Rooms.AsNoTracking()
                .OrderBy(room => room.Name)
                .Select(room => new RoomResponse(room.Id, room.Name))
                .ToListAsync(cancellationToken));

        rooms.MapGet("/{roomId:guid}/bookings", async (
            Guid roomId,
            DateTimeOffset from,
            DateTimeOffset to,
            BookingService bookingService,
            CancellationToken cancellationToken) =>
        {
            var bookings = await bookingService.ListForRoomAsync(roomId, from, to, cancellationToken);
            return Results.Ok(bookings.Select(booking => booking.ToResponse()));
        });

        return api;
    }
}
