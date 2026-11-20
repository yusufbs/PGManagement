using Microsoft.EntityFrameworkCore;
using PGManagement.Domain.Entities;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.BillId == billId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Bill)
                .ThenInclude(b => b.RoomAllocation)
            .Where(p => p.Bill.RoomAllocation.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}
