using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.Infrastructure.Repositories;

public class RoomAllocationRepository : Repository<RoomAllocation>, IRoomAllocationRepository
{
    public RoomAllocationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RoomAllocation>> GetActiveAllocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(a => a.Room)
            .Include(a => a.Tenant)
            .Where(a => a.Status == AllocationStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoomAllocation>> GetAllocationsByRoomIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(a => a.Tenant)
            .Where(a => a.RoomId == roomId && a.Status == AllocationStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomAllocation?> GetActiveAllocationByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Room)
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Status == AllocationStatus.Active, cancellationToken);
    }

    public async Task<RoomAllocation?> GetByIdWithDetailsAsync(int allocationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Room)
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.AllocationId == allocationId, cancellationToken);
    }
}
