using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Allocations;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Allocations.Commands.CreateAllocation;

public record CreateAllocationCommand(
    int RoomId,
    int TenantId,
    DateOnly CheckInDate,
    decimal SecurityDeposit
) : IRequest<AllocationResponse>;

public class CreateAllocationCommandValidator : AbstractValidator<CreateAllocationCommand>
{
    public CreateAllocationCommandValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0);
        RuleFor(x => x.TenantId).GreaterThan(0);
        RuleFor(x => x.SecurityDeposit).GreaterThanOrEqualTo(0);
    }
}

public class CreateAllocationCommandHandler : IRequestHandler<CreateAllocationCommand, AllocationResponse>
{
    private readonly IRoomAllocationRepository _allocationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IFusionCache _cache;

    public CreateAllocationCommandHandler(
        IRoomAllocationRepository allocationRepository,
        IRoomRepository roomRepository,
        ITenantRepository tenantRepository,
        IFusionCache cache)
    {
        _allocationRepository = allocationRepository;
        _roomRepository = roomRepository;
        _tenantRepository = tenantRepository;
        _cache = cache;
    }

    public async Task<AllocationResponse> Handle(CreateAllocationCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException(request.RoomId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new TenantNotFoundException(request.TenantId);

        // Check if tenant already has an active allocation
        var existingActive = await _allocationRepository.GetActiveAllocationByTenantIdAsync(request.TenantId, cancellationToken);
        if (existingActive != null)
            throw new DuplicateAllocationException();

        // Check room capacity
        var activeAllocations = await _allocationRepository.GetAllocationsByRoomIdAsync(request.RoomId, cancellationToken);
        if (activeAllocations.Count >= room.Capacity)
            throw new RoomNotAvailableException(request.RoomId);

        var allocation = new RoomAllocation
        {
            RoomId = request.RoomId,
            TenantId = request.TenantId,
            CheckInDate = request.CheckInDate,
            SecurityDeposit = request.SecurityDeposit,
            Status = AllocationStatus.Active
        };

        await _allocationRepository.AddAsync(allocation, cancellationToken);

        // If capacity reached, mark room Occupied
        if (activeAllocations.Count + 1 >= room.Capacity)
        {
            room.Status = RoomStatus.Occupied;
            _roomRepository.Update(room);
        }

        await _allocationRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Dashboard, token: cancellationToken);

        var detailed = await _allocationRepository.GetByIdWithDetailsAsync(allocation.AllocationId, cancellationToken);
        return detailed!.Adapt<AllocationResponse>();
    }
}
