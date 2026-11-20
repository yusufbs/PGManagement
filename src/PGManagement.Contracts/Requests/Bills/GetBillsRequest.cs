namespace PGManagement.Contracts.Requests.Bills;

public class GetBillsRequest
{
    public int? TenantId { get; set; }
    public int? BillMonth { get; set; }
    public int? BillYear { get; set; }
    public bool? IsPaid { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
