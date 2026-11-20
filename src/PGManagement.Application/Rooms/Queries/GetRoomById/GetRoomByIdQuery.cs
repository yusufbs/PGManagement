using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Rooms.Queries.GetRoomById;

public record GetRoomByIdQuery(int RoomId) : IRequest<RoomResponse>;

public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomResponse>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomByIdQueryHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<RoomResponse> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException(request.RoomId);

        return room.Adapt<RoomResponse>();
    }
}
