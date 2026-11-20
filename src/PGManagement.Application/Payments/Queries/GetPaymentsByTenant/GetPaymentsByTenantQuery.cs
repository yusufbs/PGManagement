using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Payments;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Payments.Queries.GetPaymentsByTenant;

public record GetPaymentsByTenantQuery(int TenantId) : IRequest<IReadOnlyList<PaymentResponse>>;

public class GetPaymentsByTenantQueryHandler : IRequestHandler<GetPaymentsByTenantQuery, IReadOnlyList<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsByTenantQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<IReadOnlyList<PaymentResponse>> Handle(GetPaymentsByTenantQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetPaymentsByTenantIdAsync(request.TenantId, cancellationToken);
        return payments.Adapt<List<PaymentResponse>>();
    }
}
