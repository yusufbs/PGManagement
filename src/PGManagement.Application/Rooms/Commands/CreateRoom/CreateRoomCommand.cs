using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(
    string RoomNumber,
    int Floor,
    string RoomType,
    int Capacity,
    decimal MonthlyRent,
    string? Amenities
) : IRequest<RoomResponse>;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number is required.")
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters.");

        RuleFor(x => x.Floor)
            .GreaterThanOrEqualTo(0).WithMessage("Floor must be 0 or greater.");

        RuleFor(x => x.Capacity)
            .InclusiveBetween(1, 20).WithMessage("Capacity must be between 1 and 20.");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0.");

        RuleFor(x => x.RoomType)
            .IsEnumName(typeof(RoomType), caseSensitive: false)
            .WithMessage("Invalid room type. Valid values: Single, Double, Triple, Dormitory.");
    }
}

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomResponse>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IFusionCache _cache;

    public CreateRoomCommandHandler(IRoomRepository roomRepository, IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _cache = cache;
    }

    public async Task<RoomResponse> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var existing = await _roomRepository.GetByRoomNumberAsync(request.RoomNumber, cancellationToken);
        if (existing != null)
            throw new DomainException($"Room with number '{request.RoomNumber}' already exists.");

        var roomType = Enum.Parse<RoomType>(request.RoomType, ignoreCase: true);

        var room = new Room
        {
            RoomNumber = request.RoomNumber,
            Floor = request.Floor,
            RoomType = roomType,
            Capacity = request.Capacity,
            MonthlyRent = request.MonthlyRent,
            Amenities = request.Amenities,
            Status = RoomStatus.Available,
            IsDeleted = false
        };

        await _roomRepository.AddAsync(room, cancellationToken);
        await _roomRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.RoomsAll, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomsAvailable, token: cancellationToken);

        return room.Adapt<RoomResponse>();
    }
}
