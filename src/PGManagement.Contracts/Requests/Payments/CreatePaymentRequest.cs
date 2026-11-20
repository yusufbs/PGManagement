namespace PGManagement.Contracts.Requests.Payments;

public class CreatePaymentRequest
{
    public required int BillId { get; set; }
    public required decimal AmountPaid { get; set; }
    public required string PaymentMode { get; set; }
    public string? TransactionRef { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Remarks { get; set; }
}
