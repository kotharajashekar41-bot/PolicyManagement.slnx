using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PolicyPlatform.Api.Middleware;
using PolicyPlatform.Application.DependencyInjection;
using PolicyPlatform.Infrastructure.DependencyInjection;
using PolicyPlatform.Infrastructure.Persistence;
using PolicyPlatform.Infrastructure.Persistence.Seed;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Chubb APAC Policy Management API";
        document.Info.Version = "v1";
        document.Info.Description =
            "BFF service for the Policy Management Platform. Contract source of truth: openapi/policy-api.yaml";
        return Task.CompletedTask;
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PolicyDbContext>("database");

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Policy API v1"));

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Dashboard");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    await PolicySeeder.SeedAsync(dbContext);
}

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
