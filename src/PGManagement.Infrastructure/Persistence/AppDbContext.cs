using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Common;
using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RoomAllocation> RoomAllocations => Set<RoomAllocation>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filters – soft delete
        modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                var updatedAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.UpdatedAt));
                if (updatedAt is not null)
                    updatedAt.CurrentValue = now;
            }
            else if (entry.State == EntityState.Added)
            {
                var createdAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.CreatedAt));
                if (createdAt is not null && createdAt.CurrentValue is DateTime dt && dt == default)
                    createdAt.CurrentValue = now;

                var updatedAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.UpdatedAt));
                if (updatedAt is not null)
                    updatedAt.CurrentValue = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
