using Microsoft.EntityFrameworkCore;
using PolicyPlatform.Domain.Entities;

namespace PolicyPlatform.Infrastructure.Persistence;

public class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
{
    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PolicyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
