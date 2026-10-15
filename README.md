# Slot Booking

A full-stack room booking system:

- **Back end:** .NET 10 Web API with Entity Framework Core and PostgreSQL
- **Front end:** Angular (standalone components, signals)
- **Orchestration:** .NET Aspire — one command starts PostgreSQL, the API, and the web app
- **Concurrency:** database-enforced protection against double booking, plus optimistic concurrency on updates
- **Errors and logging:** global exception handling (RFC 7807 ProblemDetails), Serilog, OpenTelemetry

## Run it (one click)

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 22.12+](https://nodejs.org), and [Docker Desktop](https://www.docker.com/products/docker-desktop) (running).

- **macOS / Linux:** `./run.sh`
- **Windows (PowerShell):** `./run.ps1`
- **Visual Studio / Rider:** open `SlotBooking.slnx`, set `SlotBooking.AppHost` as the startup project, press F5.

The Aspire dashboard opens in your browser. From there:

- Open the **web** resource endpoint to use the app.
- Open the **api** resource endpoint. In Development, the OpenAPI document is at `/openapi/v1.json`, and health checks are at `/health`.
- Logs, traces and metrics for every resource appear in the dashboard.

## Architecture

```
src/
├── SlotBooking.AppHost/           Aspire orchestrator: PostgreSQL + API + Angular dev server
├── SlotBooking.ServiceDefaults/   Shared OpenTelemetry, health checks, resilient HTTP defaults
└── SlotBooking.Api/
    ├── Domain/                    Exceptions that map to HTTP status codes
    ├── Models/                    Booking (business rules) and Room
    ├── Data/                      EF Core context, schema + database rules
    ├── Services/                  BookingService: use cases and concurrency handling
    ├── Endpoints/                 Minimal API routes and contracts
    └── Errors/                    Global exception handler, PostgreSQL error codes
web/slotbooking-web/               Angular app (features/, core/, shared/)
tests/sql/                         SQL checks for the database rules
```

## Concurrency: how race conditions are handled

Two different races can happen in a booking system, and each has its own guard.

### 1. Double booking (two users book the same slot)

The database refuses overlapping active bookings with an `EXCLUDE` constraint:

```sql
EXCLUDE USING gist (room_id WITH =, tstzrange(starts_at, ends_at, '[)') WITH &&)
WHERE (status = 'Active')
```

The application does not check-then-insert, because two requests could both pass that check. The database serialises the conflicting inserts, and the loser receives SQLSTATE `23P01`, which the API returns as **409 Conflict** ("Slot already booked"). Ranges are half-open, so back-to-back bookings are allowed, and cancelled bookings free their slot.

### 2. Lost updates (two users edit the same booking)

Every booking carries a version backed by PostgreSQL's `xmin` system column (`UseXminAsConcurrencyToken()`). A client reads the booking, including its version, and sends the version back with each update or cancel. EF Core adds `WHERE xmin = <version>` to the `UPDATE`:

- If nobody else changed the booking, the update applies and returns the new version.
- If someone else changed it first, zero rows match. The API returns **409 Conflict** with `currentVersion`, and the UI reloads the booking.

### Verified

The database rules were tested against PostgreSQL 16 (`tests/sql/verify-booking-rules.sql` plus a two-session race). Overlaps are rejected, back-to-back bookings and cancelled slots work, a concurrent insert waits for the first transaction and then fails, and a stale `xmin` update matches zero rows.

## Error handling and logging

- **Global exception handler** (`GlobalExceptionHandler`) converts every exception into an RFC 7807 ProblemDetails response with a `traceId`. Domain errors map to 400, 404 or 409. Unexpected errors become a generic 500 with the full exception logged.
- **Front end:** an HTTP interceptor normalises API errors, a `GlobalErrorHandler` catches anything unhandled, and the bookings page shows toasts and reloads state after conflicts.
- **Back end logging:** Serilog writes structured logs to the console, and OpenTelemetry exports them to the Aspire dashboard. Request logging records method, path, status and timing.

## API

Base path `/api`.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/rooms` | List rooms |
| `GET` | `/rooms/{roomId}/bookings?from=&to=` | Bookings overlapping a time range |
| `GET` | `/bookings/{id}` | Get one booking |
| `POST` | `/bookings` | Create a booking `{ roomId, startsAt, endsAt, guestName }` |
| `PUT` | `/bookings/{id}` | Reschedule `{ version, startsAt, endsAt, guestName }` |
| `POST` | `/bookings/{id}/cancel` | Cancel `{ version }` |

Business rules: the end must be after the start, the duration is at most 24 hours, the start must be in the future when creating, and the guest name is required (max 150 characters).

## Configuration

Aspire injects the database connection automatically. Other settings live in `src/SlotBooking.Api/appsettings*.json` (Serilog levels). Set `OTEL_EXPORTER_OTLP_ENDPOINT` to send telemetry to another collector.

## Schema notes

The schema is created with `EnsureCreated` and then the database rules are applied idempotently at startup (`DatabaseInitializer`). For production, switch to EF Core migrations (`dotnet ef migrations add`) and keep the rule SQL in a migration.

## Known limitations

- Single-tenant, no authentication. Add authentication before exposing this API.
- `EnsureCreated` is used instead of migrations (see above).
- The Angular dev server runs with `ng serve` through Aspire. A production build (`npm run build`) emits static files that need a web server and a reverse proxy for `/api`.
