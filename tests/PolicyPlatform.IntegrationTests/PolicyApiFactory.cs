using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using PolicyPlatform.Domain.Entities;
using PolicyPlatform.Domain.Enums;
using PolicyPlatform.Infrastructure.Persistence;
using Xunit;

namespace PolicyPlatform.IntegrationTests;

/// <summary>Boots the real API pipeline against an isolated, file-based SQLite database
/// (one temp file per factory instance) seeded with a small, deterministic dataset —
/// not the Bogus-random seeder used at runtime — so assertions on counts/filters are
/// stable. Pointed at via configuration rather than swapping the DbContext registration
/// after the fact: registering two providers (SqlServer then SQLite) against the same
/// DI container makes EF Core throw "services for database providers X, Y have been
/// registered" even after removing the DbContextOptions descriptor, so it's simpler to
/// just never register SqlServer in the first place for the Testing environment.</summary>
public class PolicyApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"policyplatform-test-{Guid.NewGuid():N}.db");

    public static readonly Guid ActivePropertyId = Guid.NewGuid();
    public static readonly Guid ExpiredCasualtyId = Guid.NewGuid();
    public static readonly Guid PendingMarineId = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ConfigureAppConfiguration doesn't reliably win priority against the minimal
        // hosting entry point's own config chain here, so use environment variables
        // instead — ASP.NET Core's default config always includes these, and setting
        // them before the host builds (ConfigureWebHost runs pre-Build()) is enough.
        Environment.SetEnvironmentVariable("Database__Provider", "Sqlite");
        Environment.SetEnvironmentVariable("ConnectionStrings__PolicyDb", $"Data Source={_dbPath}");
    }

    public async Task InitializeAsync()
    {
        // Touching Services forces the host (and Program.cs's migration step) to run.
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();

        var now = DateTime.UtcNow;

        dbContext.Policies.AddRange(
            new Policy
            {
                Id = ActivePropertyId,
                PolicyNumber = "POL-000001",
                PolicyholderName = "Alice Tan",
                LineOfBusiness = LineOfBusiness.Property,
                Status = PolicyStatus.Active,
                PremiumAmount = 10_000m,
                Currency = "SGD",
                EffectiveDate = DateOnly.FromDateTime(now.AddMonths(-1)),
                ExpiryDate = DateOnly.FromDateTime(now.AddDays(10)), // expiring soon
                Region = "Singapore",
                Underwriter = "Underwriter One",
                FlaggedForReview = false,
                CreatedAt = now.AddMonths(-1),
                UpdatedAt = now.AddMonths(-1)
            },
            new Policy
            {
                Id = ExpiredCasualtyId,
                PolicyNumber = "POL-000002",
                PolicyholderName = "Bob Lim",
                LineOfBusiness = LineOfBusiness.Casualty,
                Status = PolicyStatus.Expired,
                PremiumAmount = 25_000m,
                Currency = "HKD",
                EffectiveDate = DateOnly.FromDateTime(now.AddYears(-2)),
                ExpiryDate = DateOnly.FromDateTime(now.AddYears(-1)),
                Region = "Hong Kong",
                Underwriter = "Underwriter Two",
                FlaggedForReview = false,
                CreatedAt = now.AddYears(-2),
                UpdatedAt = now.AddYears(-2)
            },
            new Policy
            {
                Id = PendingMarineId,
                PolicyNumber = "POL-000003",
                PolicyholderName = "Chandra Wijaya",
                LineOfBusiness = LineOfBusiness.Marine,
                Status = PolicyStatus.Pending,
                PremiumAmount = 50_000m,
                Currency = "USD",
                EffectiveDate = DateOnly.FromDateTime(now.AddDays(5)),
                ExpiryDate = DateOnly.FromDateTime(now.AddYears(1)),
                Region = "Indonesia",
                Underwriter = "Underwriter Three",
                FlaggedForReview = false,
                CreatedAt = now,
                UpdatedAt = now
            });

        await dbContext.SaveChangesAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        // Microsoft.Data.Sqlite pools connections at the ADO.NET level, which keeps
        // the OS file handle open past SqliteConnection.Close() — clear the pool first
        // or the file delete below intermittently fails with "in use by another process".
        SqliteConnection.ClearAllPools();
        try
        {
            File.Delete(_dbPath);
            File.Delete(_dbPath + "-shm");
            File.Delete(_dbPath + "-wal");
        }
        catch (IOException)
        {
            // Best-effort cleanup of a temp file — leftover files under %TEMP% don't
            // affect test correctness and shouldn't fail the run.
        }

        return Task.CompletedTask;
    }
}
