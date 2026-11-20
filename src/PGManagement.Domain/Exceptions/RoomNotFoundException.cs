namespace PGManagement.Domain.Exceptions;

public class RoomNotFoundException : DomainException
{
    public RoomNotFoundException(int roomId)
        : base($"Room {roomId} not found") { }
}
