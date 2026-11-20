namespace PGManagement.Infrastructure.Caching;

public static class CacheKeys
{
    public const string RoomsAll = "rooms:all";
    public const string RoomsAvailable = "rooms:available";
    public const string TenantsAll = "tenants:all";
    public const string Dashboard = "dashboard:stats";

    public static string Room(int id) => $"room:{id}";
    public static string Tenant(int id) => $"tenant:{id}";
    public static string Bill(int id) => $"bill:{id}";
    public static string TenantPayments(int tenantId) => $"payments:tenant:{tenantId}";
}
