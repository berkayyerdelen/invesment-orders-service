# Investment Orders

A client with an investment account buys Apple (AAPL) stock: enter a quantity, see the total cost, confirm the order is valid, execute it, and see the confirmed result.

- **Back end:** .NET 10 Web API, Clean Architecture (Api / Application / Domain / Infrastructure / SharedKernel), PostgreSQL via EF Core, Redis cache + distributed lock
- **Front end:** Angular (standalone components, signals, reactive forms)
- **Errors and logging:** global exception handling (RFC 7807 ProblemDetails)

## Run it

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 22.12+](https://nodejs.org), Docker.

Primary way to run — Aspire starts Postgres, Redis, the API container, and the Angular container, applies EF Core migrations, and seeds demo data automatically:

```bash
dotnet run --project src/InvestmentOrders.AppHost
```

The AppHost dashboard prints the Angular app's URL (pinned to `http://localhost:4201`).

Standalone two-terminal flow still works, but now needs a local Postgres and Redis, with connection strings set in `src/InvestmentOrders.Api/appsettings.Development.json` (`ConnectionStrings:investmentorders`, `ConnectionStrings:cache`):

```bash
# Terminal 1 — API (http://localhost:5080)
dotnet run --project src/InvestmentOrders.Api

# Terminal 2 — Angular dev server (http://localhost:4200), proxies /api to the API above
cd web/slotbooking-web
npm install
npm start
```

Open `http://localhost:4200`. In Development, the OpenAPI document is at `http://localhost:5080/openapi/v1.json`.

## Architecture

```
src/
├── InvestmentOrders.Api/             Controllers, Requests/Responses, DI extensions, Program.cs
├── InvestmentOrders.Application/     Use cases: Contracts, Services, Requests/Responses, MappingExtensions
├── InvestmentOrders.Domain/          InvestmentAccount, Stock, Order; Money/Currency value objects; SufficientFundsPolicy; repository interfaces; exceptions
├── InvestmentOrders.Infrastructure/  EF Core DbContext + configurations, Postgres-backed repositories, Redis cache + distributed lock, migrations
└── InvestmentOrders.SharedKernel/    DomainException base class
web/slotbooking-web/                  Angular app (features/orders, core/)
```

Money amounts (`InvestmentAccount.CashBalance`, `Stock.Price`, `Order.PricePerShare`) are a `Money` value object (`Amount` + `Currency`) in the Domain layer — arithmetic and currency-mismatch guards live on `Money` itself, not duplicated across callers. The API still returns plain decimals (Application's mapping extensions flatten `Money.Amount` at the boundary).

The app seeds one investment account (cash balance $10,000.00) and one stock (AAPL, Apple Inc., $150.00/share) on first run.

## API

Base path `/api`.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/accounts/default` | The demo account's cash balance |
| `GET` | `/stocks/AAPL` | AAPL's name and current price |
| `POST` | `/orders/validate` | `{ accountId, symbol, quantity }` → preview: `{ isValid, pricePerShare, totalAmount, errorMessage }`. Never persists anything. |
| `POST` | `/orders` | `{ accountId, symbol, quantity }` → debits the account, persists the order, returns `{ orderId, pricePerShare, totalAmount, executedAt, remainingCashBalance }` |

Requests are validated with data annotations (`[ApiController]` returns 400 automatically). Business rules — unknown account/stock (404), insufficient funds (422) — are `DomainException`s mapped to RFC 7807 ProblemDetails by `GlobalExceptionHandler`.

## Concurrency

`AccountRepository.DebitAsync` wraps the debit in a Redis distributed lock (`Medallion.Threading.Redis`) keyed per account, so concurrent buys on the same account across API replicas are serialized. The account's row also carries an optimistic concurrency token (Postgres' native `xmin` system column via EF Core); a `DbUpdateConcurrencyException` is retried a few times as the backstop for any race the lock didn't catch. `/api/orders/validate` stays a pure, side-effect-free preview — no funds are held until `/api/orders` actually places the order.

`StockRepository.FindBySymbolAsync` cache-asides stock quotes in Redis (30s TTL), using the same distributed lock to avoid a cache-stampede on a miss.

## Tests

- `tests/InvestmentOrders.Domain.Tests`, `tests/InvestmentOrders.Application.Tests` (xUnit): funds policy, order math, and `OrderService` validate/execute paths against fake repositories.

  ```bash
  dotnet test InvestmentOrders.slnx
  ```

- `web/slotbooking-web` (Vitest via `ng test`): logger, notifications, the error interceptor, and the orders page (review → confirm → execute, including the insufficient-funds path).

  ```bash
  cd web/slotbooking-web && npm install && npx ng test --watch=false
  ```

## Known limitations

- Single-tenant, no authentication.
- One hardcoded stock (AAPL) at a fixed price — no live market data feed.
- No idempotency key on `POST /api/orders` — a client-side retry of the same request would place a second order. Not needed for a single-user demo; flagged as a deliberate omission rather than an oversight.
