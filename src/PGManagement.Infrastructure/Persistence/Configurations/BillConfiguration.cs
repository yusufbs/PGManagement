using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Persistence.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills");

        builder.HasKey(b => b.BillId);

        builder.Property(b => b.RentAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.ElectricityCharges)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.WaterCharges)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.OtherCharges)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.Discount)
            .HasColumnType("decimal(18,2)");

        // TotalAmount as a computed column
        builder.Property(b => b.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .HasComputedColumnSql(
                "[RentAmount] + [ElectricityCharges] + [WaterCharges] + [OtherCharges] - [Discount]",
                stored: true);

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        builder.Property(b => b.GeneratedAt)
            .HasColumnType("datetime2");

        // Relationship
        builder.HasOne(b => b.RoomAllocation)
            .WithMany(a => a.Bills)
            .HasForeignKey(b => b.AllocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index: one bill per allocation per month/year
        builder.HasIndex(b => new { b.AllocationId, b.BillMonth, b.BillYear })
            .IsUnique()
            .HasDatabaseName("IX_Bills_Allocation_Month_Year_Unique");
    }
}
