using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Persistence.Configurations;

public class RoomAllocationConfiguration : IEntityTypeConfiguration<RoomAllocation>
{
    public void Configure(EntityTypeBuilder<RoomAllocation> builder)
    {
        builder.ToTable("RoomAllocations");

        builder.HasKey(a => a.AllocationId);

        builder.Property(a => a.SecurityDeposit)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("datetime2");

        // Relationships
        builder.HasOne(a => a.Room)
            .WithMany(r => r.Allocations)
            .HasForeignKey(a => a.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Tenant)
            .WithMany(t => t.Allocations)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.RoomId)
            .HasDatabaseName("IX_RoomAllocations_RoomId");

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_RoomAllocations_TenantId");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_RoomAllocations_Status");
    }
}
