namespace PGManagement.Domain.Exceptions;

public class BillAlreadyExistsException : DomainException
{
    public BillAlreadyExistsException(int allocationId, int month, int year)
        : base($"A bill for allocation {allocationId} already exists for {month}/{year}") { }
}
