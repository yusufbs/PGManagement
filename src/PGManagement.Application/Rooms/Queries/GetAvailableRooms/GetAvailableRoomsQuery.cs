using MediatR;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Rooms.Queries.GetAvailableRooms;

public record GetAvailableRoomsQuery : IRequest<IReadOnlyList<RoomAvailabilityResponse>>;

public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, IReadOnlyList<RoomAvailabilityResponse>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public GetAvailableRoomsQueryHandler(IRoomRepository roomRepository, IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<RoomAvailabilityResponse>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            CacheKeys.RoomsAvailable,
            async _ =>
            {
                var rooms = await _roomRepository.GetAvailableRoomsAsync(cancellationToken);

                return (IReadOnlyList<RoomAvailabilityResponse>)rooms.Select(r =>
                {
                    var activeOccupants = r.Allocations.Count;
                    var availableSlots = Math.Max(0, r.Capacity - activeOccupants);

                    return new RoomAvailabilityResponse(
                        r.RoomId,
                        r.RoomNumber,
                        r.Floor,
                        r.RoomType.ToString(),
                        r.Capacity,
                        activeOccupants,
                        availableSlots,
                        r.MonthlyRent
                    );
                }).ToList();
            },
            token: cancellationToken
        );
    }
}
