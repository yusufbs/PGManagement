using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
}
