using MediatR;
using PGManagement.Contracts.Responses.Dashboard;
using PGManagement.Domain.Enums;
using PGManagement.Infrastructure.Caching;
using PGManagement.Infrastructure.Repositories;
using ZiggyCreatures.Caching.Fusion;

namespace PGManagement.Application.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery : IRequest<DashboardResponse>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomAllocationRepository _allocationRepository;
    private readonly IBillRepository _billRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFusionCache _cache;

    public GetDashboardQueryHandler(
        IRoomRepository roomRepository,
        IRoomAllocationRepository allocationRepository,
        IBillRepository billRepository,
        IPaymentRepository paymentRepository,
        IFusionCache cache)
    {
        _roomRepository = roomRepository;
        _allocationRepository = allocationRepository;
        _billRepository = billRepository;
        _paymentRepository = paymentRepository;
        _cache = cache;
    }

    public async Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            CacheKeys.Dashboard,
            async _ =>
            {
                var allRooms = await _roomRepository.GetAllAsync(cancellationToken);
                var totalRooms = allRooms.Count;
                var occupiedRooms = allRooms.Count(r => r.Status == RoomStatus.Occupied);
                var availableRooms = allRooms.Count(r => r.Status == RoomStatus.Available);
                var maintenanceRooms = allRooms.Count(r => r.Status == RoomStatus.Maintenance);

                var occupancyPercent = totalRooms > 0
                    ? Math.Round((decimal)occupiedRooms / totalRooms * 100, 1)
                    : 0m;

                var activeAllocations = await _allocationRepository.GetActiveAllocationsAsync(cancellationToken);
                var totalActiveTenants = activeAllocations.Count;

                var pendingBills = await _billRepository.GetPendingDuesAsync(cancellationToken);
                var totalPendingDues = pendingBills.Sum(b => b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
                var overdueBillsCount = pendingBills.Count(b => b.DueDate < DateOnly.FromDateTime(DateTime.UtcNow));

                // Current month revenue
                var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
                var now = DateTime.UtcNow;
                var monthlyRevenue = allPayments
                    .Where(p => p.PaymentDate.Month == now.Month && p.PaymentDate.Year == now.Year)
                    .Sum(p => p.AmountPaid);

                // Last 12 months revenue history
                var monthlyHistory = new List<MonthlyRevenueItem>();
                for (int i = 11; i >= 0; i--)
                {
                    var targetDate = now.AddMonths(-i);
                    var monthRevenue = allPayments
                        .Where(p => p.PaymentDate.Month == targetDate.Month && p.PaymentDate.Year == targetDate.Year)
                        .Sum(p => p.AmountPaid);

                    monthlyHistory.Add(new MonthlyRevenueItem(
                        targetDate.Month,
                        targetDate.Year,
                        monthRevenue,
                        targetDate.ToString("MMM yyyy")
                    ));
                }

                return new DashboardResponse(
                    totalRooms,
                    occupiedRooms,
                    availableRooms,
                    maintenanceRooms,
                    occupancyPercent,
                    totalActiveTenants,
                    totalPendingDues,
                    overdueBillsCount,
                    monthlyRevenue,
                    monthlyHistory
                );
            },
            token: cancellationToken
        );
    }
}
