namespace PGManagement.Contracts.Requests.Allocations;

public class CheckoutRequest
{
    public required DateOnly CheckOutDate { get; set; }
}
