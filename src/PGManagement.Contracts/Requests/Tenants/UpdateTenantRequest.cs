namespace PGManagement.Contracts.Requests.Tenants;

public class UpdateTenantRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string IdProofType { get; set; }
    public required string IdProofNumber { get; set; }
    public string? EmergencyContact { get; set; }
}
