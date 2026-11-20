using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Tenants;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string IdProofType,
    string IdProofNumber,
    string? EmergencyContact
) : IRequest<TenantResponse>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.IdProofType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.IdProofNumber).NotEmpty().MaximumLength(100);
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IFusionCache _cache;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository, IFusionCache cache)
    {
        _tenantRepository = tenantRepository;
        _cache = cache;
    }

    public async Task<TenantResponse> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await _tenantRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new DomainException($"Tenant with email '{request.Email}' already exists.");

        var tenant = new Tenant
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            IdProofType = request.IdProofType,
            IdProofNumber = request.IdProofNumber,
            EmergencyContact = request.EmergencyContact,
            IsActive = true,
            IsDeleted = false
        };

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.TenantsAll, token: cancellationToken);

        return tenant.Adapt<TenantResponse>();
    }
}
