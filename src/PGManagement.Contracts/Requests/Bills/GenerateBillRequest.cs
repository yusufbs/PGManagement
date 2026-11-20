namespace PGManagement.Contracts.Requests.Bills;

public class GenerateBillRequest
{
    public required int AllocationId { get; set; }
    public required int BillMonth { get; set; }
    public required int BillYear { get; set; }
    public required decimal ElectricityCharges { get; set; }
    public required decimal WaterCharges { get; set; }
    public required decimal OtherCharges { get; set; }
    public required decimal Discount { get; set; }
    public required DateOnly DueDate { get; set; }
    public string? Notes { get; set; }
}
