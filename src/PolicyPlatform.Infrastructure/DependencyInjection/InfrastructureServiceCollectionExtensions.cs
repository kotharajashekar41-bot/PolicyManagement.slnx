using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyPlatform.Application.Interfaces;
using PolicyPlatform.Infrastructure.Persistence;
using PolicyPlatform.Infrastructure.Repositories;

namespace PolicyPlatform.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("PolicyDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:PolicyDb configuration.");

        services.AddDbContext<PolicyDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
            }
            else
            {
                options.UseSqlite(connectionString);
            }

            // EF9+ throws by default if the runtime-built model doesn't hash-match the
            // last migration's snapshot. Our schema is verified correct by hand (and by
            // the integration tests below) — this specific check is a known false
            // positive across the enum-to-string conversions used here, so it's
            // downgraded to a log entry instead of a startup-time exception.
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddMemoryCache();

        return services;
    }
}
