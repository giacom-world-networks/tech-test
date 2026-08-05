# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Giacom tech test: an Order API over a seeded MySQL database. `README.md` defines four tasks (filter orders by status, update an order's status, create an order with validation, report profit per month for completed orders) and states that the submission is assessed by running `docker-compose down --volumes` followed by `docker-compose up` — so the containerised path must work, not just `dotnet run`.

## Commands

All solution paths are relative to the repository root; the solution lives in `src/`.

```bash
dotnet build src/Order.sln -c Release
dotnet test  src/Order.sln
dotnet test  src/Order.sln --filter "Name~GetOrderByIdAsync_TreatsNullQuantityAsZero"   # single test
dotnet test  src/Order.sln --filter "Name~GetOrdersByStatusAsync"                       # by prefix

docker compose up -d --build      # full stack; API on http://localhost:8000
docker compose up db              # database only, for running the API from an IDE
docker compose down --volumes     # tear down and discard the seeded data
```

`/health` and `/openapi/v1.json` are available alongside the `/orders` routes.

## Architecture

Four layers, each its own project, with dependencies pointing inwards:

```
Order.WebAPI  ->  Order.Service  ->  Order.Data  ->  Order.Model
 (controllers)     (business rules)   (EF Core)       (DTOs, no dependencies)
```

`Order.Data` owns EF entities that mirror the MySQL schema created by `mysql-init.sql`. `Order.Model` holds the DTOs the API returns and the request models it accepts.

**The repository projects entities straight into `Order.Model` DTOs.** There is no mapping library and no intermediate domain model. This has consequences worth knowing before editing `OrderRepository`:

- `.Include(...)` is a no-op when a query ends in `.Select(...)` into a DTO — EF ignores it. Do not add Includes to projected queries.
- Totals are computed in SQL (`SUM` over order items), not in C#.
- The shared `ToOrderSummary` expression is reused by the "all orders" and "by status" queries; keep them identical by editing that one expression.

**Identifiers are `binary(16)` in MySQL and `byte[]` on entities**, but `Guid` on DTOs and at the API boundary. Convert with `new Guid(bytes)` and `guid.ToByteArray()`. Round-tripping through these two is consistent; do not introduce `Guid.Parse`/`ToString` conversions, which produce different byte orders.

**Service failures use `OperationResult<T>`, not exceptions.** `OperationOutcome` distinguishes `Success` / `NotFound` / `Invalid`, which is what lets the controller return 404 versus 400 without extra database round trips. `OrderController.MapFailure` is the single place that translates outcomes into HTTP responses.

**Validation is deliberately split.** Shape validation lives on the request models as DataAnnotations, which `[ApiController]` turns into RFC 9457 `ValidationProblemDetails` automatically. Validation that needs the database (does this product exist?) lives in the repository and comes back as `OperationResult.Invalid`. `NotEmptyGuidAttribute` exists because `Guid` is a value type, so `[Required]` alone is satisfied by `Guid.Empty`.

Request model properties are intentionally nullable (`string? Status`, `IList<...>? Items`) so a missing JSON property is reported by `[Required]` as a validation error rather than by the JSON deserializer.

## Project configuration

- **Target framework, `Nullable`, and `ImplicitUsings` live in `src/Directory.Build.props`**, not in individual `.csproj` files.
- **Package versions live in `src/Directory.Packages.props`** (Central Package Management). `PackageReference` entries in `.csproj` files carry no `Version` attribute.
- `CentralTransitivePinningEnabled` was tried and **does not take effect in this repo** — it never writes `centralTransitiveDependencyGroups` to `project.assets.json`, even when forced on the CLI. To lift a vulnerable transitive package, add a direct `PackageReference` in the affected project and put the version in `Directory.Packages.props`. `Microsoft.OpenApi` and `SQLitePCLRaw.bundle_e_sqlite3` are referenced for exactly this reason and for no other.
- The build is warning-free. Keep it that way; nullable warnings in particular are load-bearing here.

## Gotchas

**`order_item.Quantity` is nullable in the schema.** Always use `i.Quantity ?? 0`. Using `.Value` compiles and passes tests against seeded data, then throws `Nullable object must have a value` and returns a 500 the moment a real NULL appears. This caused a production-shaped bug once already.

**`SslMode=Disabled` in the connection strings is required, not an oversight.** The MySQL connector cannot complete a TLS handshake against `mysql:5.7` and fails with `Cannot determine the frame size or a corrupted frame was received`. This predates the .NET 10 migration — the old .NET 8 build fails identically. Upgrading MySQL is not an option because `mysql-init.sql` uses `GRANT ... IDENTIFIED BY` syntax that is invalid in MySQL 8.

**Status names are compared case-insensitively via `ToLower()` inside LINQ**, because that is what EF translates to SQL `LOWER`. `ToLowerInvariant()` is used for the client-side half of the comparison; EF cannot translate it. Do not "fix" the `ToLower()` in the expression tree.

**`OrderService` is ambiguous in three different ways.** There is a `Order.Service.OrderService` class, a `OrderService.WebAPI` namespace, and a `Order.Data.Entities.OrderService` entity. `Program.cs` must say `Order.Service.OrderService` in full; test files that import both `Order.Data.Entities` and `Order.Model` must qualify `OrderItem`.

**Route order matters in `OrderController`.** `/orders/profit` is declared before `/orders/{orderId:guid}`. Literal segments outrank route parameters so it would resolve either way, but keep the ordering readable.

**`order` is a reserved word in MySQL.** Raw SQL against that table needs backticks: ``SELECT ... FROM orders.`order` ``.

## Tests

`Order.Service.Tests` are integration tests against a **real SQLite in-memory connection**, not the EF InMemory provider — that package was deliberately removed from `Order.Data`. `[SetUp]` builds a fresh database per test, so tests are isolated but each one pays for schema creation.

Use NUnit 4's constraint model (`Assert.That(actual, Is.EqualTo(expected))`) throughout; the classic model is legacy.

**`OrderContext.OnModelCreating` converts `decimal` to `double` when the SQLite provider is active**, because SQLite cannot aggregate decimals. Tests therefore do not exercise the exact decimal arithmetic that production runs against MySQL. Verify money-related changes against the real stack via `docker compose`, not just the unit tests.

## Known limitations

Not defects to fix casually — each needs a schema or infrastructure decision:

- `GET /orders` and `GET /orders/status/{name}` return the whole table; there is no pagination.
- `UpdateOrderStatusAsync` is read-modify-write with no concurrency token, so concurrent updates silently last-write-wins. There is no `rowversion` column to hook into.
- The compose stack hardcodes the database password, which is fine for the test but not a pattern to copy.
