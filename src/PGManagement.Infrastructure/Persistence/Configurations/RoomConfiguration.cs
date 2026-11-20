using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;

namespace PGManagement.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.RoomId);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.RoomType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.MonthlyRent)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Amenities)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime2");

        // Unique constraint on RoomNumber
        builder.HasIndex(r => r.RoomNumber)
            .IsUnique()
            .HasDatabaseName("IX_Rooms_RoomNumber_Unique");

        // Index on Status for filtering
        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Rooms_Status");

        // Check constraint: Capacity >= 1
        builder.ToTable(t => t.HasCheckConstraint("CK_Rooms_Capacity", "[Capacity] >= 1"));

        // Check constraint: Status must be valid values
        builder.ToTable(t => t.HasCheckConstraint("CK_Rooms_Status",
            "[Status] IN ('Available','Occupied','Maintenance')"));
    }
}
