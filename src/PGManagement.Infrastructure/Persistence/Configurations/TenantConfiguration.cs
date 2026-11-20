using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.TenantId);

        builder.Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.IdProofType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.IdProofNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.EmergencyContact)
            .HasMaxLength(200);

        builder.Property(t => t.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime2");

        // Unique constraint on Email
        builder.HasIndex(t => t.Email)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Email_Unique");

        // Index on IsActive for filtering active tenants
        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_Tenants_IsActive");
    }
}
