using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Allocations;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Allocations.Queries.GetAllocations;

public record GetAllocationsQuery : IRequest<IReadOnlyList<AllocationResponse>>;

public class GetAllocationsQueryHandler : IRequestHandler<GetAllocationsQuery, IReadOnlyList<AllocationResponse>>
{
    private readonly IRoomAllocationRepository _allocationRepository;

    public GetAllocationsQueryHandler(IRoomAllocationRepository allocationRepository)
    {
        _allocationRepository = allocationRepository;
    }

    public async Task<IReadOnlyList<AllocationResponse>> Handle(GetAllocationsQuery request, CancellationToken cancellationToken)
    {
        var allocations = await _allocationRepository.GetActiveAllocationsAsync(cancellationToken);
        return allocations.Adapt<List<AllocationResponse>>();
    }
}
