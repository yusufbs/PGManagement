using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;

namespace PGManagement.Infrastructure.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<Room?> GetByRoomNumberAsync(string roomNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Room>> GetRoomsWithAllocationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string? status = null, CancellationToken cancellationToken = default);
}
