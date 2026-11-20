using MediatR;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Rooms.Commands.DeleteRoom;

public record DeleteRoomCommand(int RoomId) : IRequest<bool>;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public DeleteRoomCommandHandler(IRoomRepository roomRepository, IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException(request.RoomId);

        _roomRepository.Delete(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAll, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Room(request.RoomId), token: cancellationToken);

        return true;
    }
}
