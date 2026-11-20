using MediatR;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Bills.Queries.GetPendingDues;

public record GetPendingDuesQuery : IRequest<IReadOnlyList<PendingDueResponse>>;

public class GetPendingDuesQueryHandler : IRequestHandler<GetPendingDuesQuery, IReadOnlyList<PendingDueResponse>>
{
    private readonly IBillRepository _billRepository;

    public GetPendingDuesQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<IReadOnlyList<PendingDueResponse>> Handle(GetPendingDuesQuery request, CancellationToken cancellationToken)
    {
        var unpaidBills = await _billRepository.GetPendingDuesAsync(cancellationToken);

        // Group unpaid bills by tenant
        var grouped = unpaidBills
            .GroupBy(b => new
            {
                b.RoomAllocation.TenantId,
                TenantName = $"{b.RoomAllocation.Tenant.FirstName} {b.RoomAllocation.Tenant.LastName}",
                b.RoomAllocation.Room.RoomNumber
            })
            .Select(g =>
            {
                var totalDue = g.Sum(b => b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
                var lastPayment = g.SelectMany(b => b.Payments).OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;

                return new PendingDueResponse(
                    g.Key.TenantId,
                    g.Key.TenantName,
                    g.Key.RoomNumber,
                    g.Count(),
                    totalDue,
                    lastPayment
                );
            })
            .OrderByDescending(p => p.TotalDue)
            .ToList();

        return grouped;
    }
}
