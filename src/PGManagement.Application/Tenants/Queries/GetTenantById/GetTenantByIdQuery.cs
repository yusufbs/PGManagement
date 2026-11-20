using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Tenants;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(int TenantId) : IRequest<TenantResponse>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantResponse>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantResponse> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetTenantWithCurrentRoomAsync(request.TenantId, cancellationToken)
            ?? throw new TenantNotFoundException(request.TenantId);

        return tenant.Adapt<TenantResponse>();
    }
}
