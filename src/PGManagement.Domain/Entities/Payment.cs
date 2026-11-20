using PGManagement.Domain.Enums;

namespace PGManagement.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? TransactionRef { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Remarks { get; set; }

    public Bill Bill { get; set; } = null!;
}
