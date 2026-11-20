namespace PGManagement.Domain.Entities;

public class Bill
{
    public int BillId { get; set; }
    public int AllocationId { get; set; }
    public int BillMonth { get; set; }
    public int BillYear { get; set; }
    public decimal RentAmount { get; set; }
    public decimal ElectricityCharges { get; set; }
    public decimal WaterCharges { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal Discount { get; set; }

    /// <summary>Computed: RentAmount + ElectricityCharges + WaterCharges + OtherCharges - Discount</summary>
    public decimal TotalAmount => RentAmount + ElectricityCharges + WaterCharges + OtherCharges - Discount;

    public DateOnly DueDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? Notes { get; set; }

    public RoomAllocation RoomAllocation { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
