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

// Add Swagger generator for interactive UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Chubb APAC Policy Management API";
        document.Info.Version = "v1";
        // fix description to point at contract openapi.yaml
        document.Info.Description =
            "BFF service for the Policy Management Platform. Contract source of truth: openapi.yaml";
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

// Add Swagger middleware so interactive UI is available at /swagger
app.UseSwagger(); // serves /swagger/v1/swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Policy API v1");
    // optional: serve UI at root by uncommenting
    // options.RoutePrefix = string.Empty;
});

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Policy API v1"));

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Dashboard");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Integration tests supply their own deterministic dataset via PolicyApiFactory
// and only need the schema created, not 220 random Bogus rows.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await PolicySeeder.SeedAsync(dbContext);
    }
}

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
