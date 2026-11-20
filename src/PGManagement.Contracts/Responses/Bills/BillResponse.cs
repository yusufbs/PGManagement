namespace PGManagement.Contracts.Responses.Bills;

public record BillResponse(
    int BillId,
    int AllocationId,
    string TenantName,
    string RoomNumber,
    int BillMonth,
    int BillYear,
    decimal RentAmount,
    decimal ElectricityCharges,
    decimal WaterCharges,
    decimal OtherCharges,
    decimal Discount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    bool IsPaid,
    DateOnly DueDate,
    DateTime GeneratedAt,
    string? Notes
);
