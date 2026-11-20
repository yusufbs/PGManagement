using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Rooms.Queries.GetRooms;

public record GetRoomsQuery(string? Status = null, int Page = 1, int PageSize = 20) : IRequest<PagedResponse<RoomResponse>>;

public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, PagedResponse<RoomResponse>>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomsQueryHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<PagedResponse<RoomResponse>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetRoomsWithAllocationsAsync(request.Status, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _roomRepository.GetCountAsync(request.Status, cancellationToken);

        var roomResponses = rooms.Adapt<List<RoomResponse>>();

        return PagedResponse<RoomResponse>.Create(roomResponses, request.Page, request.PageSize, totalCount);
    }
}
