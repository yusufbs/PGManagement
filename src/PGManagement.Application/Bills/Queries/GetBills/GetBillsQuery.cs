using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Bills.Queries.GetBills;

public record GetBillsQuery(
    int? TenantId = null,
    int? Month = null,
    int? Year = null,
    bool? IsPaid = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResponse<BillResponse>>;

public class GetBillsQueryHandler : IRequestHandler<GetBillsQuery, PagedResponse<BillResponse>>
{
    private readonly IBillRepository _billRepository;

    public GetBillsQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<PagedResponse<BillResponse>> Handle(GetBillsQuery request, CancellationToken cancellationToken)
    {
        var bills = await _billRepository.GetBillsAsync(
            request.TenantId, request.Month, request.Year, request.IsPaid,
            request.Page, request.PageSize, cancellationToken);

        var totalCount = await _billRepository.GetCountAsync(
            request.TenantId, request.Month, request.Year, request.IsPaid, cancellationToken);

        var billResponses = bills.Adapt<List<BillResponse>>();

        return PagedResponse<BillResponse>.Create(billResponses, request.Page, request.PageSize, totalCount);
    }
}
