using Serilog;
using SlotBooking.Api.Data;
using SlotBooking.Api.Endpoints;
using SlotBooking.Api.Errors;
using SlotBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Structured logging: Serilog writes to the console, and OpenTelemetry (from ServiceDefaults)
// exports the same logs to the Aspire dashboard.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

builder.AddNpgsqlDbContext<AppDbContext>("slotbookingdb");

builder.Services.AddScoped<BookingService>();
builder.Services.AddSingleton(TimeProvider.System);

// Global exception handling: every unhandled error becomes an RFC 7807 ProblemDetails response.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await DatabaseInitializer.InitializeAsync(app.Services);

var api = app.MapGroup("/api");
api.MapRoomEndpoints();
api.MapBookingEndpoints();

app.Run();
