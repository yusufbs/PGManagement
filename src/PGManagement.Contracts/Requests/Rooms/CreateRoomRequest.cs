namespace PGManagement.Contracts.Requests.Rooms;

public class CreateRoomRequest
{
    public required string RoomNumber { get; set; }
    public required int Floor { get; set; }
    public required string RoomType { get; set; }
    public required int Capacity { get; set; }
    public required decimal MonthlyRent { get; set; }
    public string? Amenities { get; set; }
}
