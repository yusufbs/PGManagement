namespace PGManagement.Domain.Exceptions;

public class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(int tenantId)
        : base($"Tenant {tenantId} not found") { }
}
