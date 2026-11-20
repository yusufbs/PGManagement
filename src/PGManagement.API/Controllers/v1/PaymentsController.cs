using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGManagement.API.Observability;
using PGManagement.Application.Payments.Commands.CreatePayment;
using PGManagement.Application.Payments.Queries.GetPaymentsByTenant;
using PGManagement.Contracts.Requests.Payments;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Payments;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly PGMetrics _metrics;

    public PaymentsController(ISender sender, PGMetrics metrics)
    {
        _sender = sender;
        _metrics = metrics;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> RecordPayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePaymentCommand(
            request.BillId,
            request.AmountPaid,
            request.PaymentMode,
            request.TransactionRef,
            request.ReceivedBy,
            request.Remarks
        );

        var response = await _sender.Send(command, cancellationToken);
        _metrics.PaymentsReceivedCounter.Add(1);
        _metrics.PaymentAmountHistogram.Record((double)request.AmountPaid);

        return Ok(ApiResponse<PaymentResponse>.Ok(response, "Payment recorded successfully."));
    }

    [HttpGet("tenant/{tenantId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentResponse>>>> GetPaymentsByTenant(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(new GetPaymentsByTenantQuery(tenantId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentResponse>>.Ok(response));
    }
}
