using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyPlatform.Domain.Entities;

namespace PolicyPlatform.Infrastructure.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PolicyNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.PolicyNumber).IsUnique();

        builder.Property(p => p.PolicyholderName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Region).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Underwriter).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();

        builder.Property(p => p.LineOfBusiness).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.PremiumAmount).HasColumnType("decimal(18,2)");

        // Composite index covers the most common filter combination (status + LOB)
        // used by both the list and summary endpoints; single-column indexes back
        // the remaining independent filters.
        builder.HasIndex(p => new { p.Status, p.LineOfBusiness });
        builder.HasIndex(p => p.Region);
        builder.HasIndex(p => p.EffectiveDate);
        builder.HasIndex(p => p.ExpiryDate);
        builder.HasIndex(p => p.FlaggedForReview);
    }
}
