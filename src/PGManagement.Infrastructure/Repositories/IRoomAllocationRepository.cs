using PGManagement.Domain.Entities;

namespace PGManagement.Infrastructure.Repositories;

public interface IRoomAllocationRepository : IRepository<RoomAllocation>
{
    Task<IReadOnlyList<RoomAllocation>> GetActiveAllocationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomAllocation>> GetAllocationsByRoomIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<RoomAllocation?> GetActiveAllocationByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<RoomAllocation?> GetByIdWithDetailsAsync(int allocationId, CancellationToken cancellationToken = default);
}
