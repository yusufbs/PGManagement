using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PGManagement.Application.Tenants.Commands.CreateTenant;
using PGManagement.Application.Tenants.Commands.UpdateTenant;
using PGManagement.Application.Tenants.Queries.GetTenantById;
using PGManagement.Application.Tenants.Queries.GetTenants;
using PGManagement.Contracts.Requests.Tenants;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Tenants;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ISender _sender;

    public TenantsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<PagedResponse<TenantResponse>>> GetTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTenantsQuery(page, pageSize), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> GetTenantById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetTenantByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<TenantResponse>.Ok(response));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTenantCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.IdProofType,
            request.IdProofNumber,
            request.EmergencyContact
        );

        var response = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTenantById), new { id = response.TenantId, version = "1.0" }, ApiResponse<TenantResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> UpdateTenant(
        int id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTenantCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.IdProofType,
            request.IdProofNumber,
            request.EmergencyContact
        );

        var response = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<TenantResponse>.Ok(response));
    }
}
