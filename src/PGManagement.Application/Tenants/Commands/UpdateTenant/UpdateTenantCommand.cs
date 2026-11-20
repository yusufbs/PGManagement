using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Tenants;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(
    int TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string IdProofType,
    string IdProofNumber,
    string? EmergencyContact
) : IRequest<TenantResponse>;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IFusionCache _cache;

    public UpdateTenantCommandHandler(ITenantRepository tenantRepository, IFusionCache cache)
    {
        _tenantRepository = tenantRepository;
        _cache = cache;
    }

    public async Task<TenantResponse> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new TenantNotFoundException(request.TenantId);

        tenant.FirstName = request.FirstName;
        tenant.LastName = request.LastName;
        tenant.Email = request.Email;
        tenant.Phone = request.Phone;
        tenant.IdProofType = request.IdProofType;
        tenant.IdProofNumber = request.IdProofNumber;
        tenant.EmergencyContact = request.EmergencyContact;

        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.TenantsAll, token: cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Tenant(request.TenantId), token: cancellationToken);

        return tenant.Adapt<TenantResponse>();
    }
}
