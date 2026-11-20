namespace PGManagement.Contracts.Responses.Bills;

public record PendingDueResponse(
    int TenantId,
    string TenantName,
    string RoomNumber,
    int MonthsOverdue,
    decimal TotalDue,
    DateTime? LastPaymentDate
);
