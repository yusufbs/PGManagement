namespace PGManagement.Contracts.Responses.Tenants;

public record TenantResponse(
    int TenantId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string IdProofType,
    string IdProofNumber,
    string? EmergencyContact,
    string? ProfilePhotoUrl,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CurrentRoomNumber);
