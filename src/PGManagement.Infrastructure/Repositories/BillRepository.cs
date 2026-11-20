using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Entities;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.Infrastructure.Repositories;

public class BillRepository : Repository<Bill>, IBillRepository
{
    public BillRepository(AppDbContext context) : base(context) { }

    public async Task<Bill?> GetByIdWithDetailsAsync(int billId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Tenant)
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Room)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.BillId == billId, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetBillsAsync(int? tenantId = null, int? month = null, int? year = null, bool? isPaid = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Tenant)
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Room)
            .Include(b => b.Payments)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(b => b.RoomAllocation.TenantId == tenantId.Value);

        if (month.HasValue)
            query = query.Where(b => b.BillMonth == month.Value);

        if (year.HasValue)
            query = query.Where(b => b.BillYear == year.Value);

        if (isPaid.HasValue)
        {
            if (isPaid.Value)
                query = query.Where(b => b.Payments.Sum(p => p.AmountPaid) >= b.TotalAmount);
            else
                query = query.Where(b => b.Payments.Sum(p => p.AmountPaid) < b.TotalAmount);
        }

        return await query
            .OrderByDescending(b => b.BillYear)
            .ThenByDescending(b => b.BillMonth)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(int? tenantId = null, int? month = null, int? year = null, bool? isPaid = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(b => b.RoomAllocation.TenantId == tenantId.Value);

        if (month.HasValue)
            query = query.Where(b => b.BillMonth == month.Value);

        if (year.HasValue)
            query = query.Where(b => b.BillYear == year.Value);

        if (isPaid.HasValue)
        {
            if (isPaid.Value)
                query = query.Where(b => b.Payments.Sum(p => p.AmountPaid) >= b.TotalAmount);
            else
                query = query.Where(b => b.Payments.Sum(p => p.AmountPaid) < b.TotalAmount);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int allocationId, int month, int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(b => b.AllocationId == allocationId && b.BillMonth == month && b.BillYear == year, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetPendingDuesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Tenant)
            .Include(b => b.RoomAllocation)
                .ThenInclude(a => a.Room)
            .Include(b => b.Payments)
            .Where(b => b.Payments.Sum(p => p.AmountPaid) < b.TotalAmount)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }
}
