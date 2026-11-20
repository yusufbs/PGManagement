export interface MonthlyRevenueItem {
  month: number;
  year: number;
  revenue: number;
  monthLabel: string;
}

export interface DashboardStats {
  totalRooms: number;
  occupiedRooms: number;
  availableRooms: number;
  maintenanceRooms: number;
  occupancyPercent: number;
  totalActiveTenants: number;
  totalPendingDues: number;
  overdueBillsCount: number;
  monthlyRevenue: number;
  last12MonthsRevenue: MonthlyRevenueItem[];
}
