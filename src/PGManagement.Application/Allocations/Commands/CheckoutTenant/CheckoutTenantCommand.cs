using MediatR;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Allocations.Commands.CheckoutTenant;

public record CheckoutTenantCommand(int AllocationId, DateOnly CheckOutDate) : IRequest<bool>;

public class CheckoutTenantCommandHandler : IRequestHandler<CheckoutTenantCommand, bool>
{
    private readonly IRoomAllocationRepository _allocationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public CheckoutTenantCommandHandler(
        IRoomAllocationRepository allocationRepository,
        IRoomRepository roomRepository,
        IFusionCache cache)
    {
        _allocationRepository = allocationRepository;
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(CheckoutTenantCommand request, CancellationToken cancellationToken)
    {
        var allocation = await _allocationRepository.GetByIdAsync(request.AllocationId, cancellationToken)
            ?? throw new DomainException($"Allocation {request.AllocationId} not found.");

        allocation.CheckOutDate = request.CheckOutDate;
        allocation.Status = AllocationStatus.CheckedOut;

        _allocationRepository.Update(allocation);

        var room = await _roomRepository.GetByIdAsync(allocation.RoomId, cancellationToken);
        if (room != null)
        {
            var remaining = await _allocationRepository.GetAllocationsByRoomIdAsync(room.RoomId, cancellationToken);
            var remainingActive = remaining.Count(a => a.AllocationId != request.AllocationId && a.Status == AllocationStatus.Active);
            if (remainingActive < room.Capacity)
            {
                room.Status = RoomStatus.Available;
                _roomRepository.Update(room);
            }
        }

        await _allocationRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Dashboard, token: cancellationToken);

        return true;
    }
}
