namespace PGManagement.Contracts.Responses.Allocations;

public record AllocationResponse(
    int AllocationId,
    int RoomId,
    string RoomNumber,
    int TenantId,
    string TenantName,
    DateOnly CheckInDate,
    DateOnly? CheckOutDate,
    decimal SecurityDeposit,
    string Status
);
