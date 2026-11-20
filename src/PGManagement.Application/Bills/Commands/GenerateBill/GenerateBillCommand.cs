using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Repositories;

namespace PGManagement.Application.Bills.Commands.GenerateBill;

public record GenerateBillCommand(
    int AllocationId,
    int BillMonth,
    int BillYear,
    decimal ElectricityCharges,
    decimal WaterCharges,
    decimal OtherCharges,
    decimal Discount,
    DateOnly DueDate,
    string? Notes
) : IRequest<BillResponse>;

public class GenerateBillCommandValidator : AbstractValidator<GenerateBillCommand>
{
    public GenerateBillCommandValidator()
    {
        RuleFor(x => x.AllocationId).GreaterThan(0);
        RuleFor(x => x.BillMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.BillYear).GreaterThan(2000);
        RuleFor(x => x.ElectricityCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WaterCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
    }
}

public class GenerateBillCommandHandler : IRequestHandler<GenerateBillCommand, BillResponse>
{
    private readonly IBillRepository _billRepository;
    private readonly IRoomAllocationRepository _allocationRepository;

    public GenerateBillCommandHandler(
        IBillRepository billRepository,
        IRoomAllocationRepository allocationRepository)
    {
        _billRepository = billRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<BillResponse> Handle(GenerateBillCommand request, CancellationToken cancellationToken)
    {
        var allocation = await _allocationRepository.GetByIdWithDetailsAsync(request.AllocationId, cancellationToken)
            ?? throw new DomainException($"Allocation {request.AllocationId} not found.");

        var exists = await _billRepository.ExistsAsync(request.AllocationId, request.BillMonth, request.BillYear, cancellationToken);
        if (exists)
            throw new BillAlreadyExistsException(request.AllocationId, request.BillMonth, request.BillYear);

        var bill = new Bill
        {
            AllocationId = request.AllocationId,
            BillMonth = request.BillMonth,
            BillYear = request.BillYear,
            RentAmount = allocation.Room.MonthlyRent,
            ElectricityCharges = request.ElectricityCharges,
            WaterCharges = request.WaterCharges,
            OtherCharges = request.OtherCharges,
            Discount = request.Discount,
            DueDate = request.DueDate,
            GeneratedAt = DateTime.UtcNow,
            Notes = request.Notes
        };

        await _billRepository.AddAsync(bill, cancellationToken);
        await _billRepository.SaveChangesAsync(cancellationToken);

        var detailedBill = await _billRepository.GetByIdWithDetailsAsync(bill.BillId, cancellationToken);
        return detailedBill!.Adapt<BillResponse>();
    }
}
