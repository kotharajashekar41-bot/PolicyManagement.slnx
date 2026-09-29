using Microsoft.Extensions.DependencyInjection;
using PolicyPlatform.Application.Interfaces;
using PolicyPlatform.Application.Services;

namespace PolicyPlatform.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPolicyService, PolicyService>();
        return services;
    }
}
