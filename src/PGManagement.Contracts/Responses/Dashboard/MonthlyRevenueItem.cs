namespace PGManagement.Contracts.Responses.Dashboard;

public record MonthlyRevenueItem(
    int Month,
    int Year,
    decimal Revenue,
    string MonthLabel
);
