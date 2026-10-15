using SlotBooking.Api.Services;

namespace SlotBooking.Api.Endpoints;

public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this RouteGroupBuilder api)
    {
        var bookings = api.MapGroup("/bookings").WithTags("Bookings");

        bookings.MapGet("/{bookingId:guid}", async (Guid bookingId, BookingService service, CancellationToken cancellationToken) =>
            (await service.GetAsync(bookingId, cancellationToken)).ToResponse());

        bookings.MapPost("/", async (CreateBookingRequest request, BookingService service, CancellationToken cancellationToken) =>
        {
            var booking = await service.CreateAsync(
                new CreateBookingCommand(request.RoomId, request.StartsAt, request.EndsAt, request.GuestName),
                cancellationToken);

            return Results.Created($"/api/bookings/{booking.Id}", booking.ToResponse());
        });

        bookings.MapPut("/{bookingId:guid}", async (
            Guid bookingId,
            UpdateBookingRequest request,
            BookingService service,
            CancellationToken cancellationToken) =>
        {
            var booking = await service.UpdateAsync(
                bookingId,
                new UpdateBookingCommand(request.Version, request.StartsAt, request.EndsAt, request.GuestName),
                cancellationToken);

            return Results.Ok(booking.ToResponse());
        });

        bookings.MapPost("/{bookingId:guid}/cancel", async (
            Guid bookingId,
            CancelBookingRequest request,
            BookingService service,
            CancellationToken cancellationToken) =>
        {
            var booking = await service.CancelAsync(bookingId, request.Version, cancellationToken);
            return Results.Ok(booking.ToResponse());
        });

        return api;
    }
}
