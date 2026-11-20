using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Allocations.Where(a => a.Status == AllocationStatus.Active))
                .ThenInclude(a => a.Room)
            .FirstOrDefaultAsync(t => t.Email == email, cancellationToken);
    }

    public async Task<Tenant?> GetTenantWithCurrentRoomAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Allocations.Where(a => a.Status == AllocationStatus.Active))
                .ThenInclude(a => a.Room)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetPagedTenantsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(t => t.Allocations.Where(a => a.Status == AllocationStatus.Active))
                .ThenInclude(a => a.Room)
            .OrderBy(t => t.FirstName)
            .ThenBy(t => t.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().CountAsync(cancellationToken);
    }
}
