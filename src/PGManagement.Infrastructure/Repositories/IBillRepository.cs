using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Repositories;

public interface IBillRepository : IRepository<Bill>
{
    Task<Bill?> GetByIdWithDetailsAsync(int billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetBillsAsync(int? tenantId = null, int? month = null, int? year = null, bool? isPaid = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(int? tenantId = null, int? month = null, int? year = null, bool? isPaid = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int allocationId, int month, int year, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetPendingDuesAsync(CancellationToken cancellationToken = default);
}
