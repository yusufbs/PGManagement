using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PaymentDate)
            .HasColumnType("datetime2");

        builder.Property(p => p.PaymentMode)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.TransactionRef)
            .HasMaxLength(100);

        builder.Property(p => p.ReceivedBy)
            .HasMaxLength(100);

        builder.Property(p => p.Remarks)
            .HasMaxLength(500);

        // Check constraint: AmountPaid must be > 0
        builder.ToTable(t => t.HasCheckConstraint("CK_Payments_AmountPaid", "[AmountPaid] > 0"));

        // Relationship
        builder.HasOne(p => p.Bill)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.BillId)
            .HasDatabaseName("IX_Payments_BillId");
    }
}
