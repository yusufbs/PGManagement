using PGManagement.Domain.Common;

namespace PGManagement.Domain.Entities;

public class Tenant : BaseEntity
{
    public int TenantId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string IdProofType { get; set; }
    public required string IdProofNumber { get; set; }
    public string? EmergencyContact { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<RoomAllocation> Allocations { get; set; } = [];
}
