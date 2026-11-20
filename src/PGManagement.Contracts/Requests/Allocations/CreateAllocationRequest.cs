namespace PGManagement.Contracts.Requests.Allocations;

public class CreateAllocationRequest
{
    public required int RoomId { get; set; }
    public required int TenantId { get; set; }
    public required DateOnly CheckInDate { get; set; }
    public required decimal SecurityDeposit { get; set; }
}
