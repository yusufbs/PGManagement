using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantWithCurrentRoomAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetPagedTenantsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
