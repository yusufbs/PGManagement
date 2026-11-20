namespace PGManagement.Domain.Exceptions;

public class RoomNotAvailableException : DomainException
{
    public RoomNotAvailableException(int roomId)
        : base($"Room {roomId} is not available for allocation") { }
}
