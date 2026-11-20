namespace PGManagement.Domain.Exceptions;

public class DuplicateAllocationException : DomainException
{
    public DuplicateAllocationException()
        : base("An active allocation already exists for this tenant or room") { }

    public DuplicateAllocationException(string message)
        : base(message) { }
}
