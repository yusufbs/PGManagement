using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PGManagement.Application.Dashboard.Queries.GetDashboard;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Dashboard;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [EnableRateLimiting("heavy-reports")]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard(CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardResponse>.Ok(response));
    }
}
