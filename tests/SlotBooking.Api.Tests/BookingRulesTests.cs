using SlotBooking.Api.Domain;
using SlotBooking.Api.Models;
using Xunit;

namespace SlotBooking.Api.Tests;

/// <summary>Pure domain rules. No database required.</summary>
public sealed class BookingRulesTests
{
    private static readonly DateTimeOffset Now = new(2030, 1, 1, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidBooking_IsActiveWithTrimmedGuest()
    {
        var booking = Booking.Create(Guid.NewGuid(), Now.AddHours(1), Now.AddHours(2), "  Alice  ", Now);

        Assert.Equal(BookingStatus.Active, booking.Status);
        Assert.Equal("Alice", booking.GuestName);
    }

    [Fact]
    public void Create_EndNotAfterStart_Throws()
    {
        Assert.Throws<ValidationException>(() =>
            Booking.Create(Guid.NewGuid(), Now.AddHours(2), Now.AddHours(2), "Alice", Now));
    }

    [Fact]
    public void Create_LongerThan24Hours_Throws()
    {
        Assert.Throws<ValidationException>(() =>
            Booking.Create(Guid.NewGuid(), Now.AddHours(1), Now.AddHours(26), "Alice", Now));
    }

    [Fact]
    public void Create_StartInThePast_Throws()
    {
        Assert.Throws<ValidationException>(() =>
            Booking.Create(Guid.NewGuid(), Now.AddHours(-1), Now.AddHours(1), "Alice", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankGuestName_Throws(string guestName)
    {
        Assert.Throws<ValidationException>(() =>
            Booking.Create(Guid.NewGuid(), Now.AddHours(1), Now.AddHours(2), guestName, Now));
    }

    [Fact]
    public void Cancel_TwiceThrowsConflict()
    {
        var booking = Booking.Create(Guid.NewGuid(), Now.AddHours(1), Now.AddHours(2), "Alice", Now);
        booking.Cancel();

        Assert.Throws<ConflictException>(() => booking.Cancel());
    }

    [Fact]
    public void Reschedule_CancelledBooking_Throws()
    {
        var booking = Booking.Create(Guid.NewGuid(), Now.AddHours(1), Now.AddHours(2), "Alice", Now);
        booking.Cancel();

        Assert.Throws<ConflictException>(() =>
            booking.Reschedule(Now.AddHours(3), Now.AddHours(4), "Alice"));
    }
}
