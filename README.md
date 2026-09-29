# Chubb APAC Policy Management Platform

A BFF service (+ optional Angular dashboard) for managing insurance policies across APAC
regions. Built for the Chubb APAC full-stack take-home assessment.

## Stack

- **Backend**: C# / ASP.NET Core (.NET 10), EF Core, Clean Architecture (Domain / Application
  / Infrastructure / Api)
- **Database**: SQL Server (matches OneHub production) in Docker / Production; SQLite for local
  `dotnet run` in Development (no local SQL Server install required)
- **API docs**: built-in `Microsoft.AspNetCore.OpenApi` generation + [Scalar](https://scalar.com)
  interactive UI. The hand-written source-of-truth contract lives at
  [`openapi/policy-api.yaml`](openapi/policy-api.yaml).
- **Frontend** (Tier 2): Angular — see [`frontend/`](frontend) if present; Tier 1 backend was
  prioritized per the assessment's own guidance.

## Running locally

### Option A — `dotnet run` (SQLite, zero setup)

```bash
cd src/PolicyPlatform.Api
dotnet run
```

Runs on `http://localhost:5255` by default (see `Properties/launchSettings.json`). On first run
it applies EF Core migrations and seeds 220+ realistic policy records via
[Bogus](https://github.com/bchavez/Bogus) into a local `policyplatform.dev.db` SQLite file
(gitignored).

- API: `http://localhost:5255/api/v1/policies`
- Interactive docs: `http://localhost:5255/scalar/v1`
- Health check: `http://localhost:5255/health`

### Option B — `docker-compose up` (SQL Server, production-shaped)

```bash
docker-compose up --build
```

Brings up SQL Server 2022 + the API together. The API waits for SQL Server's health check,
applies migrations, and seeds data on startup — same as Option A, just against the production
database engine. API is exposed on `http://localhost:5255`.

> **Note**: this compose file was written and reviewed by hand but not executed end-to-end in
> this environment (no Docker available here) — see `AI-JOURNAL.md`. If something doesn't come
> up cleanly, the most likely culprit is the SQL Server health-check command (`sqlcmd` path
> varies across mssql-server image tags).

## Running tests

```bash
dotnet test PolicyPlatform.slnx
```

19 unit tests (query parameter parsing, entity↔DTO mapping, `PolicyService` behavior against a
fake repository) + 11 integration tests (`WebApplicationFactory` against a real, isolated SQLite
database per test run, seeded with a small deterministic dataset — not the random Bogus data).

## API contract

`GET /api/v1/policies` — paginated, sorted, filtered list. Query params: `page`, `size`, `sort`
(`field,direction`, e.g. `premiumAmount,desc`), `status`, `lineOfBusiness` (`Property` |
`Casualty` | `A&H` | `Marine` — URL-encode the `&`, e.g. `A%26H`), `region`,
`effectiveDateFrom`/`effectiveDateTo`, `search` (matches policyNumber / policyholderName /
underwriter).

`GET /api/v1/policies/{id}` — single policy, 404 if missing.

`PATCH /api/v1/policies/flag` — body `{ "policyIds": ["<guid>", ...] }`, bulk-flags for review,
idempotent (re-flagging already-flagged policies is a no-op).

`GET /api/v1/policies/summary` — counts by status, total premium by line of business,
expiring-soon count (active, expiring within 30 days). Accepts the same filters as the list
endpoint. Cached for 30 seconds per distinct filter combination.

Full schema/response shapes: [`openapi/policy-api.yaml`](openapi/policy-api.yaml).

## Architecture

```
src/
  PolicyPlatform.Domain/          entities + enums, zero dependencies
  PolicyPlatform.Application/     DTOs, IPolicyRepository/IPolicyService, PolicyService
  PolicyPlatform.Infrastructure/  EF Core DbContext, migrations, repository, Bogus seeder
  PolicyPlatform.Api/             controllers, middleware, Program.cs wiring
tests/
  PolicyPlatform.UnitTests/
  PolicyPlatform.IntegrationTests/
```

Dependencies point inward: `Api → Application → Domain`, `Infrastructure → Application, Domain`.
Nothing in `Domain` or `Application` references EF Core, ASP.NET Core, or any infrastructure
concern directly — those are abstracted behind `IPolicyRepository`.

### Notable decisions

- **Database provider switch by config** (`Database:Provider`), not by environment name directly
  — `appsettings.json` defaults to SqlServer, `appsettings.Development.json` overrides to SQLite.
  Keeps `dotnet run` friction-free while staying on the production engine everywhere else.
- **Summary caching**: 30-second absolute TTL per distinct filter combination, rather than
  write-side invalidation. Flagging a policy doesn't change any summary figure (status counts /
  premium totals / expiring-soon), and nothing else writes through this API, so a short TTL
  bounds staleness without any write path needing to know the cache exists.
- **Bulk flag via `ExecuteUpdate`**: a single SQL `UPDATE ... WHERE Id IN (...)` rather than
  load-then-save per row.
- **`A&H` line of business**: not a valid C# enum member name, so the enum member is `AH` with a
  `[JsonStringEnumMemberName("A&H")]` attribute for wire compatibility; query-string parsing
  additionally accepts the bare `AH` alias (see `EnumParsing.cs`).

## What's not built (by design, given the assessment's time cap)

- Kafka producer/consumer for flag events (bonus) — documented as a deliberate cut in
  `AI-JOURNAL.md` rather than attempted under time pressure.
- Frontend bonus areas (micro-frontends, visual regression testing, virtual scrolling, i18n).
- Full-text search is a `LIKE`-based OR across three columns — fine at this data volume, would
  reach for SQL Server full-text search or a dedicated search index at real scale.
