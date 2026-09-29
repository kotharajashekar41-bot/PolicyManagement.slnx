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
- **Frontend** (Tier 2): Angular 19, standalone components, signals-based state, no
  component-level framework (no Material/PrimeNG) — see [`frontend/`](frontend).

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

### Frontend

```bash
cd frontend
npm install
npm start   # ng serve, http://localhost:4200
```

Talks to the backend at `http://localhost:5255` (see `src/environments/environment.ts`) — run
Option A above first. CORS is already configured on the API for `http://localhost:4200`.

### Option B — `docker-compose up` (SQL Server, production-shaped, full stack)

```bash
docker-compose up --build
```

Brings up SQL Server 2022, the API, and the Angular app (built and served via nginx, which also
reverse-proxies `/api/*` to the API container so there's no CORS concern in this configuration)
together. The API waits for SQL Server's health check, applies migrations, and seeds data on
startup. Frontend on `http://localhost:4200`, API directly reachable on `http://localhost:5255`.

> **Note**: this compose file was written and reviewed by hand but not executed end-to-end in
> this environment (no Docker available here) — see `AI-JOURNAL.md`. If something doesn't come
> up cleanly, the most likely culprits are the SQL Server health-check command (`sqlcmd` path
> varies across mssql-server image tags) or the frontend's `dist/frontend/browser` output path
> (confirmed locally against Angular 19's esbuild-based builder — would double-check first if a
> future Angular version changes it).

## Running tests

**Backend:**

```bash
dotnet test PolicyPlatform.slnx
```

19 unit tests (query parameter parsing, entity↔DTO mapping, `PolicyService` behavior against a
fake repository) + 11 integration tests (`WebApplicationFactory` against a real, isolated SQLite
database per test run, seeded with a small deterministic dataset — not the random Bogus data).

**Frontend:**

```bash
cd frontend
npm test -- --no-watch --browsers=ChromeHeadless
```

48 tests across services (`ThemeService` persistence/attribute application, `LocalStorageService`
failure handling, `PolicyApiService` request shaping via `HttpTestingController`), the query
store (`PolicyQueryStore` — sort toggling, filter-resets-page, URL sync via a `Router.navigate`
spy, selection), and components (`PolicyTable`, `PolicyFilters`, pagination, status badge, app
shell).

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

### Backend

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

### Frontend

```
frontend/src/app/
  core/
    models/            Policy/PolicyQuery/PolicySummary types shared across the feature
    services/           PolicyApiService (HTTP only), ThemeService, LocalStorageService
  features/policies/
    state/               PolicyQueryStore (client/URL state) + PolicyDataService (server state)
    components/          policy-table, policy-filters, policy-summary-panel,
                         bulk-actions-bar, pagination-controls — all presentational
    policy-dashboard/     route container — wires state to presentational components
  shared/components/     status-badge, empty-state, error-state, loading-skeleton, theme-toggle
```

**State management**: `PolicyQueryStore` (route-provided, not root — its state shouldn't
outlive the route) holds filters/sort/page/selection as signals and one-way syncs them to the
URL via `Router.navigate`, so any filtered/sorted/paged view is shareable and survives a
refresh. `PolicyDataService` holds server state (fetched list, summary, loading, error) and
reacts to the store's signals via `toObservable` + `switchMap`, so a rapid string of filter
edits only ever resolves the latest in-flight request. Every other component down the tree is
purely presentational — inputs/outputs only, no injected services — which is what makes the
component tests below not need a Router or HttpClient in the harness.

**Theming**: CSS custom properties on `:root`, redefined under both
`@media (prefers-color-scheme: dark)` (for `'system'`) and `[data-theme="dark"]` (for an
explicit choice) — `ThemeService` only ever needs to set or clear one attribute.

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
- Frontend bonus areas: micro-frontends, visual regression testing, dedicated E2E suite (Playwright/
  Cypress — the 48 tests here are unit/component level via Karma), virtual scrolling, i18n.
- Full-text search is a `LIKE`-based OR across three columns — fine at this data volume, would
  reach for SQL Server full-text search or a dedicated search index at real scale.
- No interactive/visual browser verification of the frontend in this environment — the person
  running this should open `http://localhost:4200` and confirm the UI looks right; see
  `AI-JOURNAL.md` for exactly what was and wasn't verified.
