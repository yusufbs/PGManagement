namespace PGManagement.Contracts.Responses.Payments;

public record PaymentResponse(
    int PaymentId,
    int BillId,
    decimal AmountPaid,
    DateTime PaymentDate,
    string PaymentMode,
    string? TransactionRef,
    string? ReceivedBy,
    string? Remarks
);
