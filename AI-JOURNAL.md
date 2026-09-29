# AI working journal

Running notes on what got accepted, challenged, or overridden while building this with Claude
Code, in build order. Not polished — a log, per the assessment brief.

## Contract & scaffolding

- Accepted Claude's proposal to hand-write `openapi/policy-api.yaml` first and implement against
  it, rather than round-tripping through NSwag codegen. Faster under time pressure, and the
  contract stays human-readable as the actual source of truth instead of a generated artifact.
- `dotnet new sln` produced a `.slnx` file, not `.sln` — new in the .NET 10 SDK. Didn't fight it,
  just adjusted commands (`dotnet build PolicyPlatform.slnx`).

## Package/version churn (the bulk of the friction)

This was the most time spent relative to value: the .NET 10 / EF Core 10 package ecosystem moved
underneath a template that still assumed net9.0/EF9 defaults.

- `Microsoft.EntityFrameworkCore.SqlServer` latest (10.0.12) only targets net10.0. **Decision**:
  retarget every project to net10.0 rather than pin EF Core back to a 9.x version — .NET 10 SDK
  was already installed, and staying current beat pinning down a version for a take-home.
- **Swashbuckle.AspNetCore vs Microsoft.OpenApi v2**: Swashbuckle 10.2.3 threw
  `TypeLoadException` at startup against the OpenApi.NET v2.7.5 that ships transitively with
  .NET 10's `Microsoft.AspNetCore.OpenApi`. Rather than pin an older Microsoft.OpenApi version
  and fight the dependency graph, switched to the framework's own `AddOpenApi()`/`MapOpenApi()`
  plus **Scalar** for the interactive UI — the pairing Microsoft actually recommends for .NET 9/10
  now. Net result is arguably cleaner than the original Swashbuckle plan.
- `dotnet-ef` global tool was version 8.0.6 (stale from a previous project) against EF Core 10
  packages — updated the tool rather than downgrading packages.

## A real bug the tests caught

- `Policy.CreatedAt`/`UpdatedAt` were originally `DateTimeOffset`. Worked fine against SQL Server
  in principle, but **SQLite cannot execute `ORDER BY` on a `DateTimeOffset` column**, and the
  default list sort is `createdAt,desc`. First manual `curl` test of `/api/v1/policies` 500'd.
  **Overrode my own initial modeling choice** and switched both fields to UTC `DateTime` — simpler
  and portable across both providers, and there's no requirement here for a non-UTC offset to be
  meaningful. Worth knowing about before it reaches a QA environment.

## Integration test infra — three iterations to get right

Getting `WebApplicationFactory<Program>` to run against an isolated per-test database took three
attempts, each teaching something about EF Core's DI integration:

1. **First attempt**: register the real SqlServer-backed `DbContextOptions` via
   `AddInfrastructure()` at startup, then override with `RemoveAll<DbContextOptions<T>>()` +
   `AddDbContext(...UseSqlite...)` in the test factory. Failed at runtime: *"Services for
   database providers 'SqlServer', 'Sqlite' have been registered in the service provider."*
   Removing the `DbContextOptions<T>` descriptor isn't enough to undo everything `AddDbContext`
   wires up when a *different* provider was configured first — even switching `AddDbContextPool`
   → `AddDbContext` didn't fix it.
2. **Second attempt**: keep the override approach but also switch the shared connection to
   `DataSource=:memory:`. Got past the dual-provider error, then hit EF9's new
   `PendingModelChangesWarning` throwing at migration time (a false positive here, confirmed by
   the schema matching manual `curl` output exactly) — suppressed via `ConfigureWarnings`.
3. **What actually worked**: stop trying to reconfigure the DbContext after the app already
   built it. Instead, point the `Testing` environment at its own file-based SQLite path via
   environment variables (`Database__Provider`, `ConnectionStrings__PolicyDb`) set *before* the
   host builds, so `AddInfrastructure()` only ever registers one provider, ever, for that host.
   Simpler and more robust than fighting DI registration order. Cost: had to disable xUnit's
   cross-class parallelization (`CollectionBehavior(DisableTestParallelization = true)`), since
   the env vars are process-wide — documented why in `AssemblyInfo.cs` rather than leaving it
   unexplained.

Also caught along the way: `HttpResponse.WriteAsJsonAsync` silently resets `Content-Type` to its
own default unless you pass the `contentType` parameter explicitly — the RFC7807
`application/problem+json` header I'd set beforehand was getting overwritten. Only surfaced
because a test asserted on the header instead of just the status code.

## Tier 2 — Angular frontend

- **Angular CLI version pin**: the current Angular CLI (would be v20+) requires Node ^22/^24/^26;
  this environment has Node 20.20.2 with no version manager available. Pinned to
  `@angular/cli@19` (supports Node 20.11+) rather than upgrading Node system-wide — a take-home
  shouldn't be changing the machine's global Node install. Angular 19.2 throughout.
- **State management, decided without being asked**: split `PolicyQueryStore` (client/URL state —
  filters, sort, page, row selection) from `PolicyDataService` (server state — fetched data,
  loading, error), each route-provided (not root). No NgRx — for one feature route with this
  shape of state, a signals-based store is less ceremony, and the brief asks for "an approach
  appropriate to the scope," not a specific library.
- **URL sync is one-directional** (signals → URL via `Router.navigate`), deliberately not wired
  back the other way after initial load, to avoid a navigate-triggers-read-triggers-navigate
  loop. Browser back/forward still works because a fresh navigation to the route re-constructs
  the store, which reads `ActivatedRoute.snapshot` fresh.
- **A real Angular forms gotcha, caught by tests**: `<option [value]="s">` (property binding)
  puts `SelectControlValueAccessor` into an id-remapping mode meant for non-string values. Mixed
  with a plain `<option value="">All statuses</option>` in the same `<select>`, the emitted value
  never matched what the test (or a real browser) expected. This isn't a test artifact — it's the
  same mechanism a real user's browser uses. Fixed by switching to interpolated `value="{{ s }}"`
  (a plain string attribute) since every option value here is genuinely just a string.
- **Two component tests simplified after an NgModel/zone.js fight**: driving `[ngModel]` +
  `(ngModelChange)` via `dispatchEvent()` in headless Karma didn't reliably fire the output even
  after accounting for NgModel's internal microtask (`tick()`, `fakeAsync`). Rather than keep
  fighting Angular forms' internal event plumbing — framework-tested elsewhere, not this app's
  logic — switched those two tests to call the component's own handler methods directly
  (`onSearchInput`, `onStatusChange`). That's what's actually specific to this component (debounce
  timing, empty-string-to-null mapping); the DOM/ngModel wiring is standard Angular.
- **No interactive browser verification**: the user declined the Chrome extension offered this
  session, and no other browser automation was available. What *was* verified: a clean production
  `ng build`, all 48 Karma/Jasmine tests green in headless Chrome, and a `curl`-based CORS
  preflight confirming `http://localhost:4200` is allowed by the API. What was **not** verified:
  that the UI actually renders correctly, that interactions feel right, or anything about visual
  correctness. A green build and green tests are necessary but not sufficient for "the feature
  works" — recommend opening `http://localhost:4200` manually against a running backend next.

## Deliberate scope decisions

- **No mocking framework** (Moq/NSubstitute) in unit tests — hand-rolled a `FakePolicyRepository`
  test double instead. For one interface with four methods, a fake is less ceremony and the
  call-count assertions (used to test the summary cache) read more plainly than
  `Verify(x => ..., Times.Once)`.
- **Caching strategy for `/summary`**: went with a flat 30s absolute TTL rather than write-side
  invalidation. Justified in code comments and README — no write path in this API changes any
  summary figure, so a short TTL is simpler and just as correct as tracking invalidation.
- **Kafka producer/consumer (bonus)**: not built. Would be the next thing tackled with more time;
  noted here rather than left unexplained, per the brief's own instruction to document what was
  cut and why. The real cost driver would be idempotent consumer handling, not the producer side.
- **`AddDbContext` over `AddDbContextPool`**: dropped context pooling. It's a real perf lever at
  higher request volumes, but at this project's scale (target: p95 < 300ms @ 50 concurrent) a
  pooled context wasn't worth the DI-registration fragility it introduced for testing (see above).
  Would reconsider if load testing showed DbContext construction as a bottleneck — it didn't get
  to that stage here.
- **Docker Compose not executed end-to-end**: no Docker available in the environment this was
  built in. The `docker-compose.yml`/`Dockerfile` follow standard multi-stage ASP.NET Core
  patterns and were reviewed by hand, but flagging this honestly rather than claiming a
  verification that didn't happen.
- **No k6/NBomber load test** — instead ran a quick ad-hoc check: 50 concurrent `curl` requests
  (via `xargs -P 50`) against `/policies`, `/policies/summary`, and `/policies/{id}`, against the
  SQLite dev database with 220 seeded rows. p95 came out around 20-55ms across all three, well
  under the 300ms target. **This is a sanity check, not a real load test** — SQLite rather than
  SQL Server, `curl` process-spawn overhead isn't the same as sustained concurrent sessions, and
  220 rows is the seeded minimum, not a stress dataset. A proper k6 run against SQL Server with a
  larger dataset is the next step before trusting this number in production.
- **Frontend bonus areas not built**: micro-frontends, visual regression testing, a dedicated E2E
  suite (Playwright/Cypress — the 48 tests here are unit/component level via Karma), virtual
  scrolling, i18n. None attempted; listed here rather than silently skipped.
- **Accessibility was implemented to the WCAG 2.1 AA patterns I know** (semantic table markup,
  `aria-sort`, labeled form controls, visible focus rings, `aria-live` region for loading/count
  changes, skip link, `prefers-reduced-motion` respected, hand-checked color-token contrast) but
  **not run through an automated checker** (axe-core, Lighthouse) or a screen reader. That's a
  real gap between "followed the patterns" and "verified compliant" — worth an axe-core pass
  before calling this AA-certified.
