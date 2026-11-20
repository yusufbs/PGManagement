using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Bills.Queries.GetBillById;

public record GetBillByIdQuery(int BillId) : IRequest<BillResponse>;

public class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, BillResponse>
{
    private readonly IBillRepository _billRepository;

    public GetBillByIdQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<BillResponse> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithDetailsAsync(request.BillId, cancellationToken)
            ?? throw new DomainException($"Bill {request.BillId} not found.");

        return bill.Adapt<BillResponse>();
    }
}
