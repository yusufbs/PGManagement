namespace PGManagement.Contracts.Responses.Rooms;

public record RoomResponse(
    int RoomId,
    string RoomNumber,
    int Floor,
    string RoomType,
    int Capacity,
    decimal MonthlyRent,
    string? Amenities,
    string Status,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public string StatusDisplayName => Status switch
    {
        "Available"   => "Available",
        "Occupied"    => "Occupied",
        "Maintenance" => "Under Maintenance",
        _             => Status
    };
}
