using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.FeatureManagement.Mvc;
using PGManagement.API.Observability;
using PGManagement.API.Services;
using PGManagement.Application.Bills.Commands.GenerateBill;
using PGManagement.Application.Bills.Queries.GetBillById;
using PGManagement.Application.Bills.Queries.GetBills;
using PGManagement.Application.Bills.Queries.GetPendingDues;
using PGManagement.Contracts.Requests.Bills;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Contracts.Responses.Common;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bills")]
public class BillsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IBillPdfService _pdfService;
    private readonly PGMetrics _metrics;

    public BillsController(ISender sender, IBillPdfService pdfService, PGMetrics metrics)
    {
        _sender = sender;
        _pdfService = pdfService;
        _metrics = metrics;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<BillResponse>>> GetBills(
        [FromQuery] int? tenantId = null,
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetBillsQuery(tenantId, month, year, isPaid, page, pageSize), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BillResponse>>> GetBillById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetBillByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<BillResponse>.Ok(response));
    }

    [HttpPost("generate")]
    [Authorize]
    [EnableRateLimiting("bill-generation")]
    public async Task<ActionResult<ApiResponse<BillResponse>>> GenerateBill(
        [FromBody] GenerateBillRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateBillCommand(
            request.AllocationId,
            request.BillMonth,
            request.BillYear,
            request.ElectricityCharges,
            request.WaterCharges,
            request.OtherCharges,
            request.Discount,
            request.DueDate,
            request.Notes
        );

        var response = await _sender.Send(command, cancellationToken);
        _metrics.BillsGeneratedCounter.Add(1);

        return CreatedAtAction(nameof(GetBillById), new { id = response.BillId, version = "1.0" }, ApiResponse<BillResponse>.Ok(response));
    }

    [HttpGet("pending-dues")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PendingDueResponse>>>> GetPendingDues(
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetPendingDuesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PendingDueResponse>>.Ok(response));
    }

    [HttpGet("{id:int}/print")]
    [FeatureGate("BillPrintingEnabled")]
    public async Task<IActionResult> PrintBill(
        int id,
        CancellationToken cancellationToken = default)
    {
        var bill = await _sender.Send(new GetBillByIdQuery(id), cancellationToken);
        var pdfBytes = _pdfService.GenerateBillPdf(bill);

        return File(pdfBytes, "application/pdf", $"Invoice-{bill.BillYear}-{bill.BillMonth:D2}-{bill.BillId:D4}.pdf");
    }
}
