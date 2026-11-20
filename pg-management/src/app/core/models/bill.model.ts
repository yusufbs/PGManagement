export interface Bill {
  billId: number;
  allocationId: number;
  billMonth: number;
  billYear: number;
  rentAmount: number;
  electricityCharges: number;
  waterCharges: number;
  otherCharges: number;
  discount: number;
  totalAmount: number;
  dueDate: string;
  generatedAt: string;
  notes?: string;
  tenantName?: string;
  roomNumber?: string;
  paidAmount?: number;
  balanceAmount?: number;
  status?: 'Paid' | 'Unpaid' | 'Partial' | string;
}

export interface PendingDue {
  tenantId: number;
  tenantName: string;
  roomNumber: string;
  monthsOverdue: number;
  totalDue: number;
  lastPaymentDate?: string;
  allocationId?: number;
  billId?: number;
}

export interface GenerateBillRequest {
  allocationId: number;
  billMonth: number;
  billYear: number;
  electricityCharges: number;
  waterCharges: number;
  otherCharges: number;
  discount: number;
  dueDate: string;
  notes?: string;
}

export interface BillFilter {
  tenantId?: number;
  billMonth?: number;
  billYear?: number;
  isPaid?: boolean;
  page?: number;
  pageSize?: number;
}
