namespace PGManagement.Contracts.Requests.Rooms;

public class GetRoomsRequest
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
