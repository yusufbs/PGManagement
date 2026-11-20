using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGManagement.Application.Allocations.Commands.CheckoutTenant;
using PGManagement.Application.Allocations.Commands.CreateAllocation;
using PGManagement.Application.Allocations.Queries.GetAllocations;
using PGManagement.Contracts.Requests.Allocations;
using PGManagement.Contracts.Responses.Allocations;
using PGManagement.Contracts.Responses.Common;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/allocations")]
public class AllocationsController : ControllerBase
{
    private readonly ISender _sender;

    public AllocationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AllocationResponse>>>> GetAllocations(
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetAllocationsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AllocationResponse>>.Ok(response));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AllocationResponse>>> CreateAllocation(
        [FromBody] CreateAllocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateAllocationCommand(
            request.RoomId,
            request.TenantId,
            request.CheckInDate,
            request.SecurityDeposit
        );

        var response = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<AllocationResponse>.Ok(response));
    }

    [HttpPatch("{id:int}/checkout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> CheckoutTenant(
        int id,
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new CheckoutTenantCommand(id, request.CheckOutDate), cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, "Tenant checked out successfully."));
    }
}
