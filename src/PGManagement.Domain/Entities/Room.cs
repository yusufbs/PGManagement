using PGManagement.Domain.Common;
using PGManagement.Domain.Enums;

namespace PGManagement.Domain.Entities;

public class Room : BaseEntity
{
    public int RoomId { get; set; }
    public required string RoomNumber { get; set; }
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    public int Capacity { get; set; }
    public decimal MonthlyRent { get; set; }
    public string? Amenities { get; set; }
    public RoomStatus Status { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<RoomAllocation> Allocations { get; set; } = [];
}
