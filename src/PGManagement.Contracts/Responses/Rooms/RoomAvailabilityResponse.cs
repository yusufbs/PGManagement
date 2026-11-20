namespace PGManagement.Contracts.Responses.Rooms;

public record RoomAvailabilityResponse(
    int RoomId,
    string RoomNumber,
    int Floor,
    string RoomType,
    int Capacity,
    int CurrentOccupants,
    int AvailableSlots,
    decimal MonthlyRent);
