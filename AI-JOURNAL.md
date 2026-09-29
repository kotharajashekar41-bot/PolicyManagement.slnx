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
