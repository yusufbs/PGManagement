using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PGManagement.Application.Rooms.Commands.CreateRoom;
using PGManagement.Application.Rooms.Commands.DeleteRoom;
using PGManagement.Application.Rooms.Commands.UpdateRoom;
using PGManagement.Application.Rooms.Commands.UpdateRoomStatus;
using PGManagement.Application.Rooms.Queries.GetAvailableRooms;
using PGManagement.Application.Rooms.Queries.GetRoomById;
using PGManagement.Application.Rooms.Queries.GetRooms;
using PGManagement.Contracts.Requests.Rooms;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Rooms;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rooms")]
public class RoomsController : ControllerBase
{
    private readonly ISender _sender;

    public RoomsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<PagedResponse<RoomResponse>>> GetRooms(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetRoomsQuery(status, page, pageSize), cancellationToken);
        return Ok(response);
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomAvailabilityResponse>>>> GetAvailableRooms(
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetAvailableRoomsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomAvailabilityResponse>>.Ok(response));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RoomResponse>>> GetRoomById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetRoomByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<RoomResponse>.Ok(response));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RoomResponse>>> CreateRoom(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateRoomCommand(
            request.RoomNumber,
            request.Floor,
            request.RoomType,
            request.Capacity,
            request.MonthlyRent,
            request.Amenities
        );

        var response = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRoomById), new { id = response.RoomId, version = "1.0" }, ApiResponse<RoomResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RoomResponse>>> UpdateRoom(
        int id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateRoomCommand(
            id,
            request.RoomNumber,
            request.Floor,
            request.RoomType,
            request.Capacity,
            request.MonthlyRent,
            request.Amenities
        );

        var response = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<RoomResponse>.Ok(response));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RoomResponse>>> UpdateRoomStatus(
        int id,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new UpdateRoomStatusCommand(id, request.Status), cancellationToken);
        return Ok(ApiResponse<RoomResponse>.Ok(response));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRoom(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteRoomCommand(id), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Room soft deleted successfully."));
    }
}
