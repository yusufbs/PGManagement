using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Contracts.Responses.Tenants;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Tenants.Queries.GetTenants;

public record GetTenantsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResponse<TenantResponse>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, PagedResponse<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<PagedResponse<TenantResponse>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetPagedTenantsAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await _tenantRepository.GetCountAsync(cancellationToken);

        var tenantResponses = tenants.Adapt<List<TenantResponse>>();

        return PagedResponse<TenantResponse>.Create(tenantResponses, request.Page, request.PageSize, totalCount);
    }
}
