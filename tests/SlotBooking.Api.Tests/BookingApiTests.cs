using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SlotBooking.Api.Tests;

/// <summary>HTTP-level checks: status codes and ProblemDetails for the failure paths the UI relies on.</summary>
[Collection(PostgresCollection.Name)]
public sealed class BookingApiTests : IDisposable
{
    private readonly PostgresFixture _postgres;
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public BookingApiTests(PostgresFixture postgres)
    {
        _postgres = postgres;
        _factory = new ApiFactory(postgres);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task OverlappingBooking_ReturnsConflictProblemDetails()
    {
        var roomId = await _postgres.RoomIdAsync("Room A");
        var start = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(40).AddHours(10), TimeSpan.Zero);

        var first = await _client.PostAsJsonAsync("/api/bookings",
            new { roomId, startsAt = start, endsAt = start.AddHours(1), guestName = "First" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/bookings",
            new { roomId, startsAt = start, endsAt = start.AddHours(1), guestName = "Second" });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var problem = JsonDocument.Parse(await second.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Slot already booked", problem.GetProperty("title").GetString());
        Assert.True(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task StaleUpdate_ReturnsConflictWithCurrentVersion()
    {
        var roomId = await _postgres.RoomIdAsync("Room B");
        var start = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(41).AddHours(10), TimeSpan.Zero);

        var created = await (await _client.PostAsJsonAsync("/api/bookings",
            new { roomId, startsAt = start, endsAt = start.AddHours(1), guestName = "Original" }))
            .Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        var version = created.GetProperty("version").GetUInt32();

        var fresh = await _client.PutAsJsonAsync($"/api/bookings/{id}",
            new { version, startsAt = start, endsAt = start.AddHours(1), guestName = "Edited" });
        Assert.Equal(HttpStatusCode.OK, fresh.StatusCode);

        var stale = await _client.PutAsJsonAsync($"/api/bookings/{id}",
            new { version, startsAt = start, endsAt = start.AddHours(1), guestName = "Stale" });

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var problem = JsonDocument.Parse(await stale.Content.ReadAsStringAsync()).RootElement;
        Assert.True(problem.TryGetProperty("currentVersion", out _));
    }

    [Fact]
    public async Task UnknownBooking_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/bookings/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

/// <summary>Runs the real API pipeline against the test PostgreSQL container.</summary>
public sealed class ApiFactory(PostgresFixture postgres) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:slotbookingdb"] = postgres.ConnectionString,
        }));
    }
}
