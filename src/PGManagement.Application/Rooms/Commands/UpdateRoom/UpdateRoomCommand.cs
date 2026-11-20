using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand(
    int RoomId,
    string RoomNumber,
    int Floor,
    string RoomType,
    int Capacity,
    decimal MonthlyRent,
    string? Amenities
) : IRequest<RoomResponse>;

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0);
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 20);
        RuleFor(x => x.MonthlyRent).GreaterThan(0);
        RuleFor(x => x.RoomType).IsEnumName(typeof(RoomType), caseSensitive: false);
    }
}

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, RoomResponse>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public UpdateRoomCommandHandler(IRoomRepository roomRepository, IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<RoomResponse> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(request.RoomId, cancellationToken)
            ?? throw new RoomNotFoundException(request.RoomId);

        var roomType = Enum.Parse<RoomType>(request.RoomType, ignoreCase: true);

        room.RoomNumber = request.RoomNumber;
        room.Floor = request.Floor;
        room.RoomType = roomType;
        room.Capacity = request.Capacity;
        room.MonthlyRent = request.MonthlyRent;
        room.Amenities = request.Amenities;

        _roomRepository.Update(room);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAll, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Room(request.RoomId), token: cancellationToken);

        return room.Adapt<RoomResponse>();
    }
}
