using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.Infrastructure.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(AppDbContext context) : base(context) { }

    public async Task<Room?> GetByRoomNumberAsync(string roomNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Allocations.Where(a => a.Status == AllocationStatus.Active))
            .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Allocations.Where(a => a.Status == AllocationStatus.Active))
            .Where(r => r.Status == RoomStatus.Available)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetRoomsWithAllocationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(r => r.Allocations.Where(a => a.Status == AllocationStatus.Active))
                .ThenInclude(a => a.Tenant)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RoomStatus>(status, true, out var roomStatus))
        {
            query = query.Where(r => r.Status == roomStatus);
        }

        return await query
            .OrderBy(r => r.RoomNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RoomStatus>(status, true, out var roomStatus))
        {
            query = query.Where(r => r.Status == roomStatus);
        }

        return await query.CountAsync(cancellationToken);
    }
}
