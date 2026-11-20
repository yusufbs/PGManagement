namespace PGManagement.Contracts.Responses.Dashboard;

public record DashboardResponse(
    int TotalRooms,
    int OccupiedRooms,
    int AvailableRooms,
    int MaintenanceRooms,
    decimal OccupancyPercent,
    int TotalActiveTenants,
    decimal TotalPendingDues,
    int OverdueBillsCount,
    decimal MonthlyRevenue,
    IReadOnlyList<MonthlyRevenueItem> Last12MonthsRevenue
);
