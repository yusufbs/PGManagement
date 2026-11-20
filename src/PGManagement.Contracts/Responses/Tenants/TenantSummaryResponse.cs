namespace PGManagement.Contracts.Responses.Tenants;

public record TenantSummaryResponse(
    int TenantId,
    string FullName,
    string Email,
    string Phone,
    string? CurrentRoom);
