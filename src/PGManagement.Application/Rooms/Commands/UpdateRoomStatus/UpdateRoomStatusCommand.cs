using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Rooms.Commands.UpdateRoomStatus;

public record UpdateRoomStatusCommand(int RoomId, string Status) : IRequest<RoomResponse>;

public class UpdateRoomStatusCommandHandler : IRequestHandler<UpdateRoomStatusCommand, RoomResponse>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public UpdateRoomStatusCommandHandler(IRoomRepository roomRepository, IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<RoomResponse> Handle(UpdateRoomStatusCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException(request.RoomId);

        if (!Enum.TryParse<RoomStatus>(request.Status, true, out var newStatus))
            throw new DomainException($"Invalid status '{request.Status}'. Valid statuses: Available, Occupied, Maintenance.");

        room.Status = newStatus;
        _roomRepository.Update(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAll, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Room(request.RoomId), token: cancellationToken);

        return room.Adapt<RoomResponse>();
    }
}
