using Microsoft.EntityFrameworkCore;
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

        services.AddDbContextPool<PolicyDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddMemoryCache();

        return services;
    }
}
