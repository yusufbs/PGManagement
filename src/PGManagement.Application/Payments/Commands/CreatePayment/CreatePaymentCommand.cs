using FluentValidation;
using Mapster;
using MediatR;
using PGManagement.Contracts.Responses.Payments;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;
using PGManagement.Domain.Exceptions;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(
    int BillId,
    decimal AmountPaid,
    string PaymentMode,
    string? TransactionRef,
    string? ReceivedBy,
    string? Remarks
) : IRequest<PaymentResponse>;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.BillId).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThan(0).WithMessage("Payment amount must be greater than 0.");
        RuleFor(x => x.PaymentMode).IsEnumName(typeof(PaymentMode), caseSensitive: false)
            .WithMessage("Valid payment modes: Cash, UPI, BankTransfer, Card.");
    }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBillRepository _billRepository;
    private readonly IFusionCache _cache;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IBillRepository billRepository,
        IFusionCache cache)
    {
        _paymentRepository = paymentRepository;
        _billRepository = billRepository;
        _cache = cache;
    }

    public async Task<PaymentResponse> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithDetailsAsync(request.BillId, cancellationToken)
            ?? throw new DomainException($"Bill {request.BillId} not found.");

        var existingPaid = bill.Payments.Sum(p => p.AmountPaid);
        var balanceDue = bill.TotalAmount - existingPaid;

        if (request.AmountPaid > balanceDue)
            throw new DomainException($"Amount {request.AmountPaid:C} exceeds balance due {balanceDue:C}.");

        var paymentMode = Enum.Parse<PaymentMode>(request.PaymentMode, ignoreCase: true);

        var payment = new Payment
        {
            BillId = request.BillId,
            AmountPaid = request.AmountPaid,
            PaymentDate = DateTime.UtcNow,
            PaymentMode = paymentMode,
            TransactionRef = request.TransactionRef,
            ReceivedBy = request.ReceivedBy,
            Remarks = request.Remarks
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.Dashboard, token: cancellationToken);

        return payment.Adapt<PaymentResponse>();
    }
}
