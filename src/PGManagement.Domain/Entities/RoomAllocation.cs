using PGManagement.Domain.Common;
using PGManagement.Domain.Enums;

namespace PGManagement.Domain.Entities;

public class RoomAllocation : BaseEntity
{
    public int AllocationId { get; set; }
    public int RoomId { get; set; }
    public int TenantId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly? CheckOutDate { get; set; }
    public decimal SecurityDeposit { get; set; }
    public AllocationStatus Status { get; set; }

    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Bill> Bills { get; set; } = [];
}
